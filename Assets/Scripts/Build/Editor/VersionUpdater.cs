using System;
using System.IO;
using UnityEditor;

namespace KK.Build.Editor
{
    internal static class VersionUpdater
    {
        public static string ChangelogPath => Path.Combine(Git.ProjectRoot, ChangelogReleaser.FileName);

        // Возвращает null, если CHANGELOG в порядке, иначе — что с ним не так (версию это не останавливает).
        public static string GetChangelogProblem()
        {
            if (!File.Exists(ChangelogPath))
                return null;

            var text = File.ReadAllText(ChangelogPath);
            if (!ChangelogReleaser.HasUnreleasedSection(text))
                return $"В {ChangelogReleaser.FileName} нет раздела «## [Unreleased]» — он не будет обновлён.";

            return ChangelogReleaser.IsUnreleasedEmpty(text)
                ? $"Раздел [Unreleased] в {ChangelogReleaser.FileName} пуст — в выпуске не будет описания изменений."
                : null;
        }

        // Единственный источник номера — PlayerSettings.bundleVersion (Application.version в игре).
        public static void Apply(BuildVersion version, DateTime date)
        {
            if (File.Exists(ChangelogPath))
            {
                var text = File.ReadAllText(ChangelogPath);
                if (ChangelogReleaser.HasUnreleasedSection(text))
                    File.WriteAllText(ChangelogPath, ChangelogReleaser.Release(text, version, date));
            }

            PlayerSettings.bundleVersion = version.ToString();
            AssetDatabase.SaveAssets();
        }
    }
}
