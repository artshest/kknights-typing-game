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
    public class TypingTextProviderFactory : MonoBehaviour, ITypingTextProviderFactory
    {
        public enum ProviderType
        {
            PredefinedText,
            PredefinedArticleUrl,
            Library
        }

        [SerializeField]
        private ProviderType _type;

        [SerializeField]
        [TextArea(5, 10)]
        private string _predefinedText;
        [SerializeField]
        [FormerlySerializedAs("_articleUrlApi")]
        [Tooltip("Ссылка на статью kknights.com, например https://kknights.com/posts/5235-valor-mortis")]
        private string _articleLink;
        [SerializeField]
        [Tooltip("Длина фрагмента статьи в символах. Абзацы не режутся, поэтому фрагмент бывает короче минимума")]
        private TextFragmentOptions _fragmentOptions = new();
        [SerializeField]
        [Min(1)]
        [Tooltip("Сколько секунд ждать ответа сайта")]
        private float _timeoutSeconds = 15;
        [SerializeField]
        private TextLibrary _textLibrary;

        private ITypingTextProvider _cachedProvider;
        private IWebTextLoader _webTextLoader;

        public ITypingTextProvider Create()
        {
            if (_cachedProvider != null)
            {
                return _cachedProvider;
            }

            _cachedProvider = _type switch
            {
                ProviderType.PredefinedText => new PredefinedTextProvider(_predefinedText),
                ProviderType.PredefinedArticleUrl => new UrlArticleTextProvider(
                    _articleLink, CreateWebTextLoader(), new TextFragmentSelector(_fragmentOptions, new System.Random())),
                ProviderType.Library => new RandomTextProvider(_textLibrary.Texts, new System.Random()),
                _ => throw new ArgumentOutOfRangeException(nameof(_type))
            };

            return _cachedProvider;
        }

        // Сайт принимает только TLS 1.3. В браузере рукопожатие делает он сам, а собственный TLS Unity
        // версию 1.3 не поддерживает, поэтому в редакторе и Windows-сборке нужен HttpClient
        private IWebTextLoader CreateWebTextLoader()
        {
            // Если конструктор провайдера бросил (плохая ссылка), загрузчик уже создан, а провайдер не закэширован.
            // Повтор не должен создавать второй HttpClient поверх неосвобождённого первого
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
            if (_cachedProvider is IDisposable provider) provider.Dispose();
            if (_webTextLoader is IDisposable loader) loader.Dispose();
            _cachedProvider = null;
            _webTextLoader = null;
        }
    }
}
