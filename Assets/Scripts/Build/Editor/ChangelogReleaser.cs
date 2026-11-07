using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KK.Build.Editor
{
    // Формат Keep a Changelog: пункты копятся в «## [Unreleased]», при выпуске раздел получает номер и дату.
    public static class ChangelogReleaser
    {
        public const string FileName = "CHANGELOG.md";

        // Lookahead вместо «\r?$»: перевод строки не должен попасть в совпадение, иначе при замене потеряется \r.
        private static readonly Regex UnreleasedHeading =
            new Regex(@"^## \[Unreleased\][ \t]*(?=\r?$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private static readonly Regex AnySectionHeading = new Regex(@"^## ", RegexOptions.Multiline);

        public static bool HasUnreleasedSection(string text) => UnreleasedHeading.IsMatch(text);

        public static bool IsUnreleasedEmpty(string text)
        {
            var heading = FindUnreleased(text);
            var bodyStart = heading.Index + heading.Length;
            var next = AnySectionHeading.Match(text, bodyStart);
            var bodyEnd = next.Success ? next.Index : text.Length;
            return string.IsNullOrWhiteSpace(text.Substring(bodyStart, bodyEnd - bodyStart));
        }

        public static string Release(string text, BuildVersion version, DateTime date)
        {
            var heading = FindUnreleased(text);
            if (text.Contains($"## [{version}]"))
                throw new InvalidOperationException($"В {FileName} уже есть раздел версии {version}.");

            var newline = text.Contains("\r\n") ? "\r\n" : "\n";
            var released = $"## [{version}] - {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
            return text.Substring(0, heading.Index)
                   + "## [Unreleased]" + newline + newline + released
                   + text.Substring(heading.Index + heading.Length);
        }

        private static Match FindUnreleased(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var match = UnreleasedHeading.Match(text);
            if (!match.Success)
                throw new InvalidOperationException($"В {FileName} нет раздела «## [Unreleased]».");

            return match;
        }
    }
}
