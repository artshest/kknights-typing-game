using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Infrastructure.Web;
using System.Threading;

namespace KK.Infrastructure.Texts.KKnights
{
    public class UrlArticleTextProvider : ITypingTextProvider
    {
        private readonly string _url;
        private readonly IWebTextLoader _loader;

        public UrlArticleTextProvider(string url, IWebTextLoader loader)
        {
            _url = url;
            _loader = loader;
        }

        public async UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            string json = await _loader.LoadAsync(_url, ct);
            return KKnightsPostParser.Parse(json).Paragraphs[0];
        }
    }
}
