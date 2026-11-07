using UnityEditor;
using UnityEditor.Build;

namespace KK.Build.Editor
{
    // Настройки WebGL, которые отличаются у Dev- и Release-сборок. Их нельзя держать в профиле:
    // «Customize player settings» снимает копию всех Player Settings, и профиль перестаёт видеть
    // изменения глобальных (в том числе bundleVersion). Поэтому тулза ставит их на время сборки и откатывает.
    internal readonly struct WebGLBuildSettings
    {
        private readonly WebGLCompressionFormat _compression;
        private readonly bool _decompressionFallback;
        private readonly bool _nameFilesAsHashes;
        private readonly bool _dataCaching;
        private readonly Il2CppCompilerConfiguration _il2CppConfiguration;

        private WebGLBuildSettings(
            WebGLCompressionFormat compression,
            bool decompressionFallback,
            bool nameFilesAsHashes,
            bool dataCaching,
            Il2CppCompilerConfiguration il2CppConfiguration)
        {
            _compression = compression;
            _decompressionFallback = decompressionFallback;
            _nameFilesAsHashes = nameFilesAsHashes;
            _dataCaching = dataCaching;
            _il2CppConfiguration = il2CppConfiguration;
        }

        // Release: Brotli с запасной распаковкой в JS — на случай, если хостинг (itch.io) не отдаёт
        // заголовок Content-Encoding для .br;
        // имена-хэши, чтобы браузер не взял из кэша старые .data/.wasm после обновления.
        // Dev: без сжатия и с отладочным IL2CPP — сборка быстрее, стек вызовов читаемый.
        public static WebGLBuildSettings For(bool development) => development
            ? new WebGLBuildSettings(WebGLCompressionFormat.Disabled, false, false, false, Il2CppCompilerConfiguration.Debug)
            : new WebGLBuildSettings(WebGLCompressionFormat.Brotli, true, true, true, Il2CppCompilerConfiguration.Master);

        public static WebGLBuildSettings Capture() => new WebGLBuildSettings(
            PlayerSettings.WebGL.compressionFormat,
            PlayerSettings.WebGL.decompressionFallback,
            PlayerSettings.WebGL.nameFilesAsHashes,
            PlayerSettings.WebGL.dataCaching,
            PlayerSettings.GetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL));

        public void Apply()
        {
            PlayerSettings.WebGL.compressionFormat = _compression;
            PlayerSettings.WebGL.decompressionFallback = _decompressionFallback;
            PlayerSettings.WebGL.nameFilesAsHashes = _nameFilesAsHashes;
            PlayerSettings.WebGL.dataCaching = _dataCaching;
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, _il2CppConfiguration);
        }

        public override string ToString() =>
            $"сжатие {_compression}, распаковка в JS {YesNo(_decompressionFallback)}, " +
            $"имена-хэши {YesNo(_nameFilesAsHashes)}, кэш данных {YesNo(_dataCaching)}, IL2CPP {_il2CppConfiguration}";

        private static string YesNo(bool value) => value ? "да" : "нет";
    }
}
