using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KK.Build.Editor
{
    public sealed class BuildInfo
    {
        public string ProductName;
        public BuildVersion Version;
        public string Commit;
        public bool? HasUncommittedChanges;
        public string ProfileName;
        public string Platform;
        public bool? Development;
        public string PlatformSettings;
        public DateTime StartedAt;
        public string Result;
        public TimeSpan Duration;
        public long ShippedSize;
        public int Errors;
        public int Warnings;
        public string ArchivePath;
        public long ArchiveSize;
        public List<(string Path, long Size)> Files = new List<(string, long)>();
        public List<string> PreBuildWarnings = new List<string>();
    }

    public static class BuildInfoFormatter
    {
        private static readonly NumberFormatInfo CommaDecimal = new NumberFormatInfo { NumberDecimalSeparator = "," };
        private static readonly string[] SizeUnits = { "Б", "КБ", "МБ", "ГБ" };

        // Метаданные SemVer через «+»: не участвуют в сравнении версий, но видны в меню dev-сборки.
        public static string FullVersion(BuildVersion version, string commit) =>
            string.IsNullOrEmpty(commit) ? version.ToString() : $"{version}+{commit}";

        public static string FormatSize(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentOutOfRangeException(nameof(bytes));

            double value = bytes;
            var unit = 0;
            while (value >= 1024 && unit < SizeUnits.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return unit == 0
                ? $"{bytes} {SizeUnits[0]}"
                : $"{value.ToString("0.0", CommaDecimal)} {SizeUnits[unit]}";
        }

        public static string Format(BuildInfo info)
        {
            var text = new StringBuilder();
            text.AppendLine($"{info.ProductName} {FullVersion(info.Version, info.Commit)}");
            text.AppendLine($"Профиль: {info.ProfileName} ({info.Platform})");
            text.AppendLine($"Development: {YesNoUnknown(info.Development)}");
            if (!string.IsNullOrEmpty(info.PlatformSettings))
                text.AppendLine($"Настройки платформы: {info.PlatformSettings}");
            text.AppendLine($"Коммит: {info.Commit ?? "неизвестен"}{ChangesNote(info.HasUncommittedChanges)}");
            text.AppendLine($"Начало: {info.StartedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            text.AppendLine($"Результат: {info.Result}");
            text.AppendLine($"Длительность: {info.Duration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}");
            if (info.ShippedSize > 0)
                text.AppendLine($"Размер для игрока: {FormatSize(info.ShippedSize)}");
            text.AppendLine($"Ошибок: {info.Errors}, предупреждений: {info.Warnings}");

            if (!string.IsNullOrEmpty(info.ArchivePath))
                text.AppendLine($"Архив: {info.ArchivePath} ({FormatSize(info.ArchiveSize)})");

            if (info.Files.Count > 0)
            {
                text.AppendLine("Файлы:");
                foreach (var (path, size) in info.Files)
                    text.AppendLine($"  {path} — {FormatSize(size)}");
            }

            if (info.PreBuildWarnings.Count > 0)
            {
                text.AppendLine("Предупреждения перед сборкой:");
                foreach (var warning in info.PreBuildWarnings)
                    text.AppendLine($"  - {warning}");
            }

            return text.ToString();
        }

        private static string YesNoUnknown(bool? value) => value == null ? "неизвестно" : value.Value ? "да" : "нет";

        private static string ChangesNote(bool? hasChanges) =>
            hasChanges == true ? " (+ незакоммиченные изменения)" : string.Empty;
    }
}
