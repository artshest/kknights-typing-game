using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace KK.Build.Editor
{
    // Нет git в PATH или проект не в репозитории — возвращает null: сборка от этого не должна падать.
    internal static class Git
    {
        private const int TimeoutMilliseconds = 10000;

        public static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        public static string TryGetShortCommit() => Run("rev-parse --short HEAD");

        public static bool? HasUncommittedChanges()
        {
            var status = Run("status --porcelain");
            return status == null ? (bool?)null : status.Length > 0;
        }

        private static string Run(string arguments)
        {
            var startInfo = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = ProjectRoot,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            try
            {
                using var process = Process.Start(startInfo);
                if (process == null)
                    return null;

                var output = process.StandardOutput.ReadToEnd();
                if (!process.WaitForExit(TimeoutMilliseconds))
                {
                    process.Kill();
                    return null;
                }

                return process.ExitCode == 0 ? output.Trim() : null;
            }
            catch (Exception e) when (e is Win32Exception || e is InvalidOperationException)
            {
                return null;
            }
        }
    }
}
