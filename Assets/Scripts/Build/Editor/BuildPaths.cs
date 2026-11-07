using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

namespace KK.Build.Editor
{
    // Раскладка одной сборки:
    // Builds/{профиль}/{игра}_{версия}_{время}/
    //     Game/          — то, что собрал Unity (у WebGL index.html лежит прямо здесь)
    //     build-info.txt — отчёт
    //     {игра}_{версия}_{время}.zip — содержимое Game/ для itch.io
    public static class BuildPaths
    {
        public const string BuildsFolderName = "Builds";
        public const string GameFolderName = "Game";
        public const string ReportFileName = "build-info.txt";

        public static string GetBuildName(string productName, BuildVersion version, DateTime time) =>
            $"{SanitizeFileName(productName)}_{version}_{time.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture)}";

        public static string GetBuildDirectory(string projectRoot, string profileName, string buildName) =>
            Path.Combine(projectRoot, BuildsFolderName, SanitizeFileName(profileName), buildName);

        public static string GetGameDirectory(string buildDirectory) =>
            Path.Combine(buildDirectory, GameFolderName);

        // Для Windows Unity ждёт путь к .exe, для WebGL — папку.
        public static string GetPlayerLocation(string buildDirectory, BuildTarget target, string productName)
        {
            var gameDirectory = GetGameDirectory(buildDirectory);
            return target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64
                ? Path.Combine(gameDirectory, SanitizeFileName(productName) + ".exe")
                : gameDirectory;
        }

        public static string GetArchivePath(string buildDirectory, string buildName) =>
            Path.Combine(buildDirectory, buildName + ".zip");

        public static string GetReportPath(string buildDirectory) =>
            Path.Combine(buildDirectory, ReportFileName);

        // Пробелы тоже заменяются: имя архива уходит в командную строку (butler) и в URL.
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя не может быть пустым.", nameof(name));

            var invalid = Path.GetInvalidFileNameChars();
            var result = new StringBuilder(name.Length);
            foreach (var c in name.Trim())
                result.Append(char.IsWhiteSpace(c) || Array.IndexOf(invalid, c) >= 0 ? '_' : c);

            return result.ToString();
        }
    }
}
