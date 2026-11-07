using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace KK.Build.Editor
{
    public sealed class BuildWindow : EditorWindow
    {
        private const string SelectedProfilesKeySuffix = ".KK.Build.SelectedProfiles";
        private const string CreateArchiveKeySuffix = ".KK.Build.CreateArchive";

        private readonly HashSet<string> _selectedGuids = new HashSet<string>();
        private string _versionText;
        private bool _createArchive;
        private BuildCheckResult _projectChecks;
        private readonly Dictionary<string, BuildCheckResult> _profileChecks = new Dictionary<string, BuildCheckResult>();
        private string _lastReport;
        private Vector2 _scroll;

        [MenuItem("KK/Build")]
        private static void Open() => GetWindow<BuildWindow>("KK Build");

        private void OnEnable()
        {
            _versionText = PlayerSettings.bundleVersion;
            _createArchive = EditorPrefs.GetBool(PlayerSettings.productGUID + CreateArchiveKeySuffix, true);
            _selectedGuids.Clear();
            foreach (var guid in EditorPrefs.GetString(PlayerSettings.productGUID + SelectedProfilesKeySuffix, string.Empty)
                         .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                _selectedGuids.Add(guid);

            LoadLastReport();
            RefreshChecks();
        }

        // Проверки зависят от внешнего состояния (git, сохранённость сцены) — обновляем при возврате в окно.
        private void OnFocus() => RefreshChecks();

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawVersion();
            EditorGUILayout.Space();
            var profiles = DrawProfiles();
            EditorGUILayout.Space();
            DrawChecks(profiles);
            EditorGUILayout.Space();
            DrawBuildButton(profiles);
            EditorGUILayout.Space();
            DrawLastReport();
            EditorGUILayout.EndScrollView();
        }

        private void DrawVersion()
        {
            EditorGUILayout.LabelField("Версия", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Текущая (bundleVersion)", PlayerSettings.bundleVersion);

            _versionText = EditorGUILayout.TextField("Новая", _versionText).Trim();
            var isValid = BuildVersion.TryParse(_versionText, out var version);

            using (new EditorGUILayout.HorizontalScope())
            {
                var basis = isValid ? version : BuildVersion.TryParse(PlayerSettings.bundleVersion, out var current) ? current : default;
                if (GUILayout.Button("+patch"))
                    SetVersionText(basis.NextPatch());
                if (GUILayout.Button("+minor"))
                    SetVersionText(basis.NextMinor());
                if (GUILayout.Button("+major"))
                    SetVersionText(basis.NextMajor());
            }

            if (!isValid)
                EditorGUILayout.HelpBox("Формат X.Y.Z, без ведущих нулей.", MessageType.Error);

            using (new EditorGUI.DisabledScope(!isValid || _versionText == PlayerSettings.bundleVersion))
            {
                if (GUILayout.Button("Применить версию"))
                    ApplyVersion(version);
            }
        }

        private void SetVersionText(BuildVersion version)
        {
            _versionText = version.ToString();
            GUI.FocusControl(null);
        }

        private void ApplyVersion(BuildVersion version)
        {
            var problem = VersionUpdater.GetChangelogProblem();
            if (problem != null && !EditorUtility.DisplayDialog("CHANGELOG", problem, "Всё равно применить", "Отмена"))
                return;

            try
            {
                VersionUpdater.Apply(version, DateTime.Now);
                ShowNotification(new GUIContent($"Версия {version}. Закоммить: chore: версия {version}"));
            }
            catch (InvalidOperationException e)
            {
                EditorUtility.DisplayDialog("Версия не изменена", e.Message, "OK");
            }

            RefreshChecks();
        }

        private List<BuildProfile> DrawProfiles()
        {
            EditorGUILayout.LabelField("Профили", EditorStyles.boldLabel);
            var allProfiles = BuildProfile.GetAllBuildProfiles().OrderBy(p => p.name).ToList();
            if (allProfiles.Count == 0)
                EditorGUILayout.HelpBox("Профилей нет — создайте в File > Build Profiles.", MessageType.Info);

            var selected = new List<BuildProfile>();
            foreach (var profile in allProfiles)
            {
                var guid = GetGuid(profile);
                var label = $"{profile.name}  ({BuildProfileInfo.GetTarget(profile)}{(BuildProfileInfo.IsDevelopment(profile) == true ? ", Development" : string.Empty)})";
                var wasSelected = _selectedGuids.Contains(guid);
                var isSelected = EditorGUILayout.ToggleLeft(label, wasSelected);
                if (isSelected != wasSelected)
                {
                    if (isSelected)
                        _selectedGuids.Add(guid);
                    else
                        _selectedGuids.Remove(guid);
                    EditorPrefs.SetString(PlayerSettings.productGUID + SelectedProfilesKeySuffix, string.Join(",", _selectedGuids));
                    RefreshChecks();
                }

                if (isSelected)
                    selected.Add(profile);
            }

            var createArchive = EditorGUILayout.ToggleLeft("Упаковать в zip (для itch.io)", _createArchive);
            if (createArchive != _createArchive)
            {
                _createArchive = createArchive;
                EditorPrefs.SetBool(PlayerSettings.productGUID + CreateArchiveKeySuffix, createArchive);
            }

            return selected;
        }

        private void DrawChecks(List<BuildProfile> profiles)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Проверки", EditorStyles.boldLabel);
                if (GUILayout.Button("Обновить", GUILayout.Width(80)))
                    RefreshChecks();
            }

            var checks = GetChecks(profiles);
            foreach (var error in checks.Errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (var warning in checks.Warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            if (checks.Errors.Count == 0 && checks.Warnings.Count == 0)
                EditorGUILayout.HelpBox("Замечаний нет.", MessageType.Info);
        }

        private void DrawBuildButton(List<BuildProfile> profiles)
        {
            var canBuild = profiles.Count > 0 && GetChecks(profiles).CanBuild;
            using (new EditorGUI.DisabledScope(!canBuild))
            {
                if (GUILayout.Button($"Собрать отмеченные ({profiles.Count})", GUILayout.Height(32)))
                {
                    // Сборка внутри OnGUI ломает раскладку окна (EndLayoutGroup) — запускаем после кадра GUI.
                    var toBuild = profiles.ToList();
                    EditorApplication.delayCall += () => BuildProfiles(toBuild);
                }
            }

            if (_versionText != PlayerSettings.bundleVersion)
                EditorGUILayout.HelpBox($"Соберётся версия {PlayerSettings.bundleVersion}: новая не применена.", MessageType.Info);
        }

        private void DrawLastReport()
        {
            EditorGUILayout.LabelField("Последняя сборка", EditorStyles.boldLabel);
            if (string.IsNullOrEmpty(_lastReport))
            {
                EditorGUILayout.LabelField("Сборок ещё не было.");
                return;
            }

            EditorGUILayout.SelectableLabel(_lastReport, EditorStyles.textArea,
                GUILayout.Height(EditorStyles.textArea.CalcHeight(new GUIContent(_lastReport), position.width - 20)));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Открыть папку"))
                    BuildRunner.RevealInExplorer(BuildRunner.LastReportPath);
                if (GUILayout.Button("Открыть отчёт") && File.Exists(BuildRunner.LastReportPath))
                    EditorUtility.OpenWithDefaultApp(BuildRunner.LastReportPath);
            }
        }

        private void BuildProfiles(List<BuildProfile> profiles)
        {
            RefreshChecks();
            var checks = GetChecks(profiles);
            if (!checks.CanBuild)
                return;

            if (checks.Warnings.Count > 0)
            {
                var message = string.Join(Environment.NewLine, checks.Warnings.Select(w => "• " + w));
                if (!EditorUtility.DisplayDialog("Есть предупреждения", message, "Собрать", "Отмена"))
                    return;
            }

            var reports = new List<string>();
            var failed = 0;
            foreach (var profile in profiles)
            {
                try
                {
                    var outcome = BuildRunner.Run(profile, _createArchive,
                        BuildCommands.Merge(_projectChecks, _profileChecks[GetGuid(profile)]));
                    reports.Add(outcome.ReportText);
                    if (!outcome.Succeeded)
                        failed++;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    reports.Add($"{profile.name}: {e.Message}");
                    failed++;
                }
            }

            _lastReport = string.Join(Environment.NewLine, reports);
            ShowNotification(new GUIContent(failed == 0 ? "Сборка завершена" : $"Не удалось: {failed} из {profiles.Count}"));
            RefreshChecks();
            Repaint();
        }

        private void RefreshChecks()
        {
            _projectChecks = PreBuildChecks.CheckProject();
            _profileChecks.Clear();
            foreach (var profile in BuildProfile.GetAllBuildProfiles())
            {
                var guid = GetGuid(profile);
                if (_selectedGuids.Contains(guid))
                    _profileChecks[guid] = PreBuildChecks.CheckProfile(profile);
            }

            Repaint();
        }

        private BuildCheckResult GetChecks(List<BuildProfile> profiles)
        {
            var result = _projectChecks ?? new BuildCheckResult();
            foreach (var profile in profiles)
            {
                if (_profileChecks.TryGetValue(GetGuid(profile), out var profileChecks))
                    result = BuildCommands.Merge(result, profileChecks);
            }

            return result;
        }

        private void LoadLastReport()
        {
            var path = BuildRunner.LastReportPath;
            _lastReport = File.Exists(path) ? File.ReadAllText(path) : null;
        }

        private static string GetGuid(BuildProfile profile) =>
            AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile));
    }
}
