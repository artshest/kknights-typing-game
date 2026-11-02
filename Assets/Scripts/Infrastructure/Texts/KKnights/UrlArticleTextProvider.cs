using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Infrastructure.Web;
using System;
using System.Threading;

namespace KK.Infrastructure.Texts.KKnights
{
    public class UrlArticleTextProvider : ITypingTextProvider
    {
        private readonly string _apiUrl;
        private readonly IWebTextLoader _loader;

        public UrlArticleTextProvider(string articleLink, IWebTextLoader loader)
        {
            if (!KKnightsArticleLink.TryParseSlug(articleLink, out string slug))
                throw new ArgumentException($"Ссылка не похожа на статью kknights.com: «{articleLink}»", nameof(articleLink));

            _apiUrl = KKnightsArticleLink.ToApiUrl(slug);
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public async UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            string json = await _loader.LoadAsync(_apiUrl, ct);
            return KKnightsPostParser.Parse(json).Paragraphs[0];
        }
    }
}
