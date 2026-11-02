using KK.App.Texts;
using KK.Infrastructure.Texts;
using KK.Infrastructure.Texts.KKnights;
using KK.Infrastructure.Web;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KK.Bootstrap
{
    public class TypingTextProviderFactory : MonoBehaviour
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
                ProviderType.PredefinedArticleUrl => new UrlArticleTextProvider(_articleLink, CreateWebTextLoader()),
                ProviderType.Library => new RandomTextProvider(_textLibrary.Texts, new System.Random()),
                _ => throw new ArgumentOutOfRangeException(nameof(_type))
            };

            return _cachedProvider;
        }

        // Сайт принимает только TLS 1.3. В браузере рукопожатие делает он сам, а собственный TLS Unity
        // версию 1.3 не поддерживает, поэтому в редакторе и Windows-сборке нужен HttpClient
        private IWebTextLoader CreateWebTextLoader()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _webTextLoader = new UnityWebRequestTextLoader();
#else
            _webTextLoader = new HttpClientTextLoader();
#endif
            return _webTextLoader;
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
