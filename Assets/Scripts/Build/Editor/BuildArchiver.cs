using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace KK.Build.Editor
{
    public static class BuildArchiver
    {
        // Папки, которые Unity кладёт рядом с игрой с пометкой «не отдавать игрокам»: отладочные символы Burst и IL2CPP.
        private static readonly string[] ExcludedFolderSuffixes =
        {
            "_BurstDebugInformation_DoNotShip",
            "_BackUpThisFolder_ButDontShipItWithYourGame",
        };

        public static bool ShouldExclude(string relativePath)
        {
            var segments = relativePath.Split('/', '\\');
            // Последний сегмент — имя файла, проверяются только папки.
            for (var i = 0; i < segments.Length - 1; i++)
            {
                foreach (var suffix in ExcludedFolderSuffixes)
                {
                    if (segments[i].EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        // Файлы для игрока: относительный путь с прямыми слэшами и полный путь.
        public static IEnumerable<(string RelativePath, string FullPath)> EnumerateShippedFiles(string sourceDirectory)
        {
            foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                // Прямые слэши обязательны в zip: с обратными часть распаковщиков видит не папки, а имена файлов с «\».
                var relativePath = Path.GetRelativePath(sourceDirectory, file).Replace('\\', '/');
                if (!ShouldExclude(relativePath))
                    yield return (relativePath, file);
            }
        }

        // BuildReport.totalSize считает и отладочные папки (у IL2CPP — больше гигабайта), поэтому размер — свой.
        public static long GetShippedSize(string sourceDirectory) =>
            EnumerateShippedFiles(sourceDirectory).Sum(f => new FileInfo(f.FullPath).Length);

        // Файлы кладутся в корень архива (не внутрь папки Game): itch.io ищет index.html в корне.
        public static void CreateZip(string sourceDirectory, string zipPath)
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using var stream = File.Create(zipPath);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
            foreach (var (relativePath, fullPath) in EnumerateShippedFiles(sourceDirectory))
            {
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(fullPath);
                fileStream.CopyTo(entryStream);
            }
        }
    }
}
