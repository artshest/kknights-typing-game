using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Compilation;
using UnityEngine.SceneManagement;

namespace KK.Build.Editor
{
    public sealed class BuildCheckResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool CanBuild => Errors.Count == 0;
    }

    // Ошибки блокируют сборку (собрать правильно нельзя), предупреждения — нет: решает разработчик.
    internal static class PreBuildChecks
    {
        // Поле TypingTextProviderFactory. Ищется текстом в файле сцены: тулза не ссылается на игровые сборки
        // и не открывает сцены — это сбросило бы текущую сцену редактора.
        private const string DebugTextMarker = "_useDebugText: 1";
        private const string McpRuntimeAssemblyPrefix = "MCPForUnity";

        public static BuildCheckResult CheckProject()
        {
            var result = new BuildCheckResult();

            if (!BuildVersion.TryParse(PlayerSettings.bundleVersion, out _))
                result.Errors.Add($"Версия «{PlayerSettings.bundleVersion}» не в формате X.Y.Z.");

            switch (Git.HasUncommittedChanges())
            {
                case null:
                    result.Warnings.Add("Не удалось получить состояние git: коммит в отчёт не попадёт.");
                    break;
                case true:
                    result.Warnings.Add("Есть незакоммиченные изменения: сборка не совпадёт с коммитом из отчёта.");
                    break;
            }

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                    result.Warnings.Add($"Сцена «{scene.name}» не сохранена: в сборку попадёт версия с диска.");
            }

            // Сборки для плеера считаются для активной платформы, но asmdef пакета от платформы не зависит.
            var mcpAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Where(a => a.name.StartsWith(McpRuntimeAssemblyPrefix, StringComparison.Ordinal))
                .Select(a => a.name)
                .ToList();
            if (mcpAssemblies.Count > 0)
                result.Warnings.Add($"В сборку попадёт {string.Join(", ", mcpAssemblies)} (см. stage-7-platforms.md).");

            return result;
        }

        public static BuildCheckResult CheckProfile(BuildProfile profile)
        {
            var result = new BuildCheckResult();
            var target = BuildProfileInfo.GetTarget(profile);
            var development = BuildProfileInfo.IsDevelopment(profile);

            if (target == BuildTarget.NoTarget)
                result.Errors.Add($"{profile.name}: не удалось прочитать платформу профиля.");

            if (development == null)
            {
                var message = $"{profile.name}: не удалось прочитать флаг Development.";
                // Для WebGL от флага зависят сжатие и IL2CPP — без него собрать правильно нельзя.
                if (target == BuildTarget.WebGL)
                    result.Errors.Add(message);
                else
                    result.Warnings.Add(message);
            }
            else if (development.Value && profile.name.IndexOf("Release", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Warnings.Add($"{profile.name}: включён Development Build в релизном профиле.");
            }

            foreach (var scenePath in GetScenePaths(profile))
            {
                if (File.Exists(scenePath) && File.ReadAllText(scenePath).Contains(DebugTextMarker))
                    result.Warnings.Add($"{profile.name}: в сцене {Path.GetFileName(scenePath)} включён отладочный текст (_useDebugText).");
            }

            return result;
        }

        private static IEnumerable<string> GetScenePaths(BuildProfile profile)
        {
            var scenes = profile.overrideGlobalScenes ? profile.scenes : EditorBuildSettings.scenes;
            return scenes.Where(s => s.enabled).Select(s => s.path);
        }
    }
}
