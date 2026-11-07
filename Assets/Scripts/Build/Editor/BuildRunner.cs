using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KK.Build.Editor
{
    public sealed class BuildOutcome
    {
        public bool Succeeded;
        public string BuildDirectory;
        public string ReportPath;
        public string ArchivePath;
        public string ReportText;
    }

    public static class BuildRunner
    {
        // Игра читает хэш через Resources.Load<TextAsset>("BuildCommit"). Файл живёт только на время сборки
        // (и в .gitignore на случай падения редактора посреди неё).
        private const string ResourcesFolder = "Assets/Resources";
        private const string CommitAssetPath = ResourcesFolder + "/BuildCommit.txt";

        private const string LastReportKeySuffix = ".KK.Build.LastReport";

        public static string LastReportPath
        {
            get => EditorPrefs.GetString(PlayerSettings.productGUID + LastReportKeySuffix, string.Empty);
            private set => EditorPrefs.SetString(PlayerSettings.productGUID + LastReportKeySuffix, value);
        }

        // Проверки здесь не повторяются, кроме блокирующих: решение «собирать ли с предупреждениями» — у вызывающего.
        public static BuildOutcome Run(BuildProfile profile, bool createArchive, BuildCheckResult checks)
        {
            if (!checks.CanBuild)
                throw new InvalidOperationException("Сборка невозможна: " + string.Join(" ", checks.Errors));

            var version = BuildVersion.Parse(PlayerSettings.bundleVersion);
            var productName = PlayerSettings.productName;
            var target = BuildProfileInfo.GetTarget(profile);
            var development = BuildProfileInfo.IsDevelopment(profile);
            var startedAt = DateTime.Now;

            var buildName = BuildPaths.GetBuildName(productName, version, startedAt);
            var buildDirectory = BuildPaths.GetBuildDirectory(Git.ProjectRoot, profile.name, buildName);
            Directory.CreateDirectory(buildDirectory);

            var info = new BuildInfo
            {
                ProductName = productName,
                Version = version,
                Commit = Git.TryGetShortCommit(),
                HasUncommittedChanges = Git.HasUncommittedChanges(),
                ProfileName = profile.name,
                Platform = target.ToString(),
                Development = development,
                StartedAt = startedAt,
            };
            info.PreBuildWarnings.AddRange(checks.Warnings);

            var report = BuildWithTemporarySettings(profile, target, development == true, info, buildDirectory, productName);

            var summary = report.summary;
            info.Result = summary.result.ToString();
            info.Duration = summary.totalTime;
            info.Errors = summary.totalErrors;
            info.Warnings = summary.totalWarnings;

            var outcome = new BuildOutcome
            {
                Succeeded = summary.result == BuildResult.Succeeded,
                BuildDirectory = buildDirectory,
                ReportPath = BuildPaths.GetReportPath(buildDirectory),
            };

            var gameDirectory = BuildPaths.GetGameDirectory(buildDirectory);
            if (outcome.Succeeded)
            {
                info.ShippedSize = BuildArchiver.GetShippedSize(gameDirectory);
                if (target == BuildTarget.WebGL)
                    AddWebGLFiles(info, gameDirectory);

                if (createArchive)
                {
                    outcome.ArchivePath = BuildPaths.GetArchivePath(buildDirectory, buildName);
                    BuildArchiver.CreateZip(gameDirectory, outcome.ArchivePath);
                    info.ArchivePath = Path.GetFileName(outcome.ArchivePath);
                    info.ArchiveSize = new FileInfo(outcome.ArchivePath).Length;
                }
            }

            outcome.ReportText = BuildInfoFormatter.Format(info);
            File.WriteAllText(outcome.ReportPath, outcome.ReportText);
            LastReportPath = outcome.ReportPath;
            return outcome;
        }

        private static BuildReport BuildWithTemporarySettings(
            BuildProfile profile,
            BuildTarget target,
            bool development,
            BuildInfo info,
            string buildDirectory,
            string productName)
        {
            var isWebGL = target == BuildTarget.WebGL;
            var previousWebGL = WebGLBuildSettings.Capture();
            var createdResourcesFolder = false;
            try
            {
                if (isWebGL)
                {
                    var settings = WebGLBuildSettings.For(development);
                    settings.Apply();
                    info.PlatformSettings = settings.ToString();
                }

                createdResourcesFolder = WriteCommitAsset(info.Commit);

                return BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
                {
                    buildProfile = profile,
                    locationPathName = BuildPaths.GetPlayerLocation(buildDirectory, target, productName),
                    options = BuildOptions.None,
                });
            }
            finally
            {
                DeleteCommitAsset(createdResourcesFolder);
                if (isWebGL)
                    previousWebGL.Apply();
            }
        }

        private static bool WriteCommitAsset(string commit)
        {
            if (string.IsNullOrEmpty(commit))
                return false;

            var createdFolder = !AssetDatabase.IsValidFolder(ResourcesFolder);
            if (createdFolder)
                AssetDatabase.CreateFolder("Assets", "Resources");

            File.WriteAllText(Path.Combine(Git.ProjectRoot, CommitAssetPath), commit);
            AssetDatabase.ImportAsset(CommitAssetPath);
            return createdFolder;
        }

        private static void DeleteCommitAsset(bool deleteFolder)
        {
            if (AssetDatabase.AssetPathExists(CommitAssetPath))
                AssetDatabase.DeleteAsset(CommitAssetPath);

            if (deleteFolder)
                AssetDatabase.DeleteAsset(ResourcesFolder);
        }

        // .data и .wasm — основной вес WebGL-сборки, их размер стоит видеть после каждой сборки.
        private static void AddWebGLFiles(BuildInfo info, string gameDirectory)
        {
            var buildFolder = Path.Combine(gameDirectory, "Build");
            if (!Directory.Exists(buildFolder))
                return;

            info.Files.AddRange(Directory.GetFiles(buildFolder)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.Length)
                .Select(f => ("Build/" + f.Name, f.Length)));
        }

        public static void RevealInExplorer(string path)
        {
            if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                EditorUtility.RevealInFinder(path);
            else
                Debug.LogWarning($"[KK.Build] Не найдено: {path}");
        }
    }
}
