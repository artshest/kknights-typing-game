using KK.App.Texts;

namespace KK.Infrastructure.Texts.KKnights
{
    // Те же правила, что у UrlArticleTextProvider: ссылка, прошедшая проверку, не уронит его конструктор
    public class KKnightsArticleLinkValidator : IArticleLinkValidator
    {
        public bool IsValid(string link)
        {
            return KKnightsArticleLink.TryParseSlug(link, out _);
        }
    }
}
