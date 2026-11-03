using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Core.Texts;
using KK.Infrastructure.Web;
using System;
using System.Threading;

namespace KK.Infrastructure.Texts.KKnights
{
    public class UrlArticleTextProvider : ITypingTextProvider
    {
        private readonly string _apiUrl;
        private readonly IWebTextLoader _loader;
        private readonly TextFragmentSelector _selector;

        public UrlArticleTextProvider(string articleLink, IWebTextLoader loader, TextFragmentSelector selector)
        {
            if (!KKnightsArticleLink.TryParseSlug(articleLink, out string slug))
                throw new ArgumentException($"Ссылка не похожа на статью kknights.com: «{articleLink}»", nameof(articleLink));

            _apiUrl = KKnightsArticleLink.ToApiUrl(slug);
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public async UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            string json = await _loader.LoadAsync(_apiUrl, ct);
            return _selector.Select(KKnightsPostParser.Parse(json).Paragraphs);
        }
    }
}
