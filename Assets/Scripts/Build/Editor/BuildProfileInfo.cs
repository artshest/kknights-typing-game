using UnityEditor;
using UnityEditor.Build.Profile;

namespace KK.Build.Editor
{
    // Публичный API профиля (6000.6) не отдаёт платформу и флаг Development — читаем сериализованные поля.
    // Если Unity их переименует, IsDevelopment вернёт null, а GetTarget — NoTarget; сборка это заметит.
    internal static class BuildProfileInfo
    {
        public static BuildTarget GetTarget(BuildProfile profile)
        {
            using var serialized = new SerializedObject(profile);
            var property = serialized.FindProperty("m_BuildTarget");
            return property == null ? BuildTarget.NoTarget : (BuildTarget)property.intValue;
        }

        public static bool? IsDevelopment(BuildProfile profile)
        {
            using var serialized = new SerializedObject(profile);
            var property = serialized.FindProperty("m_PlatformBuildProfile.m_Development");
            return property?.boolValue;
        }

        public static bool IsWebGL(BuildProfile profile) => GetTarget(profile) == BuildTarget.WebGL;
    }
}
