using KK.App.Texts;
using KK.Infrastructure.Texts;
using KK.Infrastructure.Texts.KKnights;
using System;
using UnityEngine;

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
        private string _articleUrlApi;
        [SerializeField]
        private TextLibrary _textLibrary;

        private ITypingTextProvider _cachedProvider;

        public ITypingTextProvider Create()
        {
            if (_cachedProvider != null)
            {
                return _cachedProvider;
            }

            _cachedProvider = _type switch
            {
                ProviderType.PredefinedText => new PredefinedTextProvider(_predefinedText),
                ProviderType.PredefinedArticleUrl => new UrlArticleTextProvider(_articleUrlApi),
                ProviderType.Library => new RandomTextProvider(_textLibrary.Texts, new System.Random()),
                _ => throw new ArgumentOutOfRangeException(nameof(_type))
            };

            return _cachedProvider;
        }

        private void OnDisable()
        {
            if (_cachedProvider is IDisposable d) d.Dispose();
            _cachedProvider = null;
        }
    }
}
