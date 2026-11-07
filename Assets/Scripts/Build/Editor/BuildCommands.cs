using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace KK.Build.Editor
{
    // Unity.exe -batchmode -quit -projectPath . -executeMethod KK.Build.Editor.BuildCommands.Build
    //     -profile WebGL-Release[,Windows-Release] [-version 0.1.0] [-noZip] [-logFile build.log]
    // Код выхода: 0 — все сборки успешны, 1 — ошибка сборки, 2 — неверные аргументы или проверки.
    public static class BuildCommands
    {
        private const int ExitSuccess = 0;
        private const int ExitBuildFailed = 1;
        private const int ExitBadInput = 2;

        public static void Build()
        {
            int exitCode;
            try
            {
                exitCode = Run(Environment.GetCommandLineArgs());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                exitCode = ExitBuildFailed;
            }

            // Без явного Exit флаг -quit всегда даёт 0, и CI не заметит провал.
            EditorApplication.Exit(exitCode);
        }

        private static int Run(string[] args)
        {
            if (!TryGetValue(args, "-profile", out var profileArgument))
                return Fail("Не указан -profile (имя профиля или несколько через запятую).");

            var profiles = new List<BuildProfile>();
            foreach (var name in SplitList(profileArgument))
            {
                var profile = BuildProfile.GetAllBuildProfiles().FirstOrDefault(p => p.name == name);
                if (profile == null)
                    return Fail($"Профиль «{name}» не найден.");
                profiles.Add(profile);
            }

            if (TryGetValue(args, "-version", out var versionArgument))
            {
                if (!BuildVersion.TryParse(versionArgument, out var version))
                    return Fail($"Версия «{versionArgument}» не в формате X.Y.Z.");

                if (PlayerSettings.bundleVersion != version.ToString())
                {
                    var changelogProblem = VersionUpdater.GetChangelogProblem();
                    if (changelogProblem != null)
                        Debug.LogWarning("[KK.Build] " + changelogProblem);
                    VersionUpdater.Apply(version, DateTime.Now);
                }
            }

            var createArchive = !HasFlag(args, "-noZip");
            var projectChecks = PreBuildChecks.CheckProject();
            var allSucceeded = true;
            foreach (var profile in profiles)
            {
                var checks = Merge(projectChecks, PreBuildChecks.CheckProfile(profile));
                foreach (var warning in checks.Warnings)
                    Debug.LogWarning("[KK.Build] " + warning);
                if (!checks.CanBuild)
                    return Fail(string.Join(Environment.NewLine, checks.Errors));

                var outcome = BuildRunner.Run(profile, createArchive, checks);
                Debug.Log("[KK.Build]" + Environment.NewLine + outcome.ReportText);
                allSucceeded &= outcome.Succeeded;
            }

            return allSucceeded ? ExitSuccess : ExitBuildFailed;
        }

        public static BuildCheckResult Merge(BuildCheckResult first, BuildCheckResult second)
        {
            var result = new BuildCheckResult();
            result.Errors.AddRange(first.Errors);
            result.Errors.AddRange(second.Errors);
            result.Warnings.AddRange(first.Warnings);
            result.Warnings.AddRange(second.Warnings);
            return result;
        }

        public static bool TryGetValue(string[] args, string name, out string value)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase) && !args[i + 1].StartsWith("-"))
                {
                    value = args[i + 1];
                    return true;
                }
            }

            value = null;
            return false;
        }

        public static bool HasFlag(string[] args, string name) =>
            args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<string> SplitList(string value) =>
            value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

        private static int Fail(string message)
        {
            Debug.LogError("[KK.Build] " + message);
            return ExitBadInput;
        }
    }
}
