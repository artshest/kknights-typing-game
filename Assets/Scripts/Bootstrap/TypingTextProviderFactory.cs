using KK.App.Texts;
using KK.Core.Texts;
using KK.Infrastructure.Texts;
using KK.Infrastructure.Texts.KKnights;
using KK.Infrastructure.Web;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KK.Bootstrap
{
    // Настройки провайдеров из инспектора. Что загружать, выбирает игрок (TextSource), а кэширование —
    // в CachedTextProviderFactory, чтобы его можно было проверить тестами без MonoBehaviour
    public class TypingTextProviderFactory : MonoBehaviour, ITypingTextProviderFactory
    {
        [SerializeField]
        private TextLibrary _textLibrary;
        [SerializeField]
        [Tooltip("Длина фрагмента статьи в символах. Абзацы не режутся, поэтому фрагмент бывает короче минимума")]
        private TextFragmentOptions _fragmentOptions = new();
        [SerializeField]
        [Min(1)]
        [Tooltip("Сколько секунд ждать ответа сайта")]
        private float _timeoutSeconds = 15;

        [SerializeField]
        [Tooltip("Вместо выбранного игроком источника всегда давать отладочный текст")]
        private bool _useDebugText;
        [SerializeField]
        [FormerlySerializedAs("_predefinedText")]
        [TextArea(5, 10)]
        private string _debugText;

        private CachedTextProviderFactory _cache;
        private IWebTextLoader _webTextLoader;

        public ITypingTextProvider Create(TextSource source)
        {
            _cache ??= new CachedTextProviderFactory(CreateProvider);
            return _cache.Create(source);
        }

        private ITypingTextProvider CreateProvider(TextSource source)
        {
            if (_useDebugText)
                return new PredefinedTextProvider(_debugText);

            return source.Kind switch
            {
                TextSourceKind.Library => new RandomTextProvider(_textLibrary.Texts, new System.Random()),
                TextSourceKind.Article => new UrlArticleTextProvider(
                    source.ArticleLink, CreateWebTextLoader(), new TextFragmentSelector(_fragmentOptions, new System.Random())),
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "Неизвестный источник текста")
            };
        }

        // Сайт принимает только TLS 1.3. В браузере рукопожатие делает он сам, а собственный TLS Unity
        // версию 1.3 не поддерживает, поэтому в редакторе и Windows-сборке нужен HttpClient
        private IWebTextLoader CreateWebTextLoader()
        {
            // Загрузчик один на все статьи: провайдер пересоздаётся при смене ссылки, а второй HttpClient
            // поверх неосвобождённого первого не нужен
            if (_webTextLoader == null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                _webTextLoader = new UnityWebRequestTextLoader();
#else
                _webTextLoader = new HttpClientTextLoader();
#endif
            }
            // В поле — сам загрузчик, а не обёртка: освобождать в OnDisable нужно именно его
            return new TimeoutWebTextLoader(_webTextLoader, TimeSpan.FromSeconds(_timeoutSeconds));
        }

        private void OnDisable()
        {
            // Загрузчик создала фабрика, а не провайдер, поэтому и освобождает его фабрика
            _cache?.Dispose();
            if (_webTextLoader is IDisposable loader) loader.Dispose();
            _cache = null;
            _webTextLoader = null;
        }
    }
}
