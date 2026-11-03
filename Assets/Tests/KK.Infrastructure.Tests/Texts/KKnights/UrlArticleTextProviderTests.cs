using System;
using KK.Core.Texts;
using KK.Infrastructure.Tests.Fakes;
using KK.Infrastructure.Texts.KKnights;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    public class UrlArticleTextProviderTests
    {
        private const string ArticleLink = "https://kknights.com/posts/5235-valor-mortis";

        private static TextFragmentSelector CreateSelector(int minLength = 300, int maxLength = 600) =>
            new TextFragmentSelector(new TextFragmentOptions { MinLength = minLength, MaxLength = maxLength }, new Random(0));

        [Test]
        public void Constructor_NotArticleLink_Throws()
        {
            var loader = new FakeWebTextLoader("{}");

            Assert.Throws<ArgumentException>(() => new UrlArticleTextProvider("https://example.com/", loader, CreateSelector()));
        }

        [Test]
        public void GetTextAsync_ArticleLink_RequestsApiUrl()
        {
            var loader = new FakeWebTextLoader(KKnightsFixtures.Load(KKnightsFixtures.Document));
            var provider = new UrlArticleTextProvider(ArticleLink, loader, CreateSelector());

            provider.GetTextAsync().GetAwaiter().GetResult();

            Assert.That(loader.RequestedUrls, Is.EqualTo(new[] { "https://kknights.com/api/v1/post/5235-valor-mortis/" }));
        }

        [Test]
        public void Constructor_NullSelector_Throws()
        {
            var loader = new FakeWebTextLoader("{}");

            Assert.Throws<ArgumentNullException>(() => new UrlArticleTextProvider(ArticleLink, loader, null));
        }

        [Test]
        public void GetTextAsync_ReturnsParagraphOfArticle()
        {
            string json = KKnightsFixtures.Load(KKnightsFixtures.Document);
            // Минимум 1 — любой абзац уже достаточно длинный, поэтому селектор вернёт ровно один целый абзац
            var provider = new UrlArticleTextProvider(ArticleLink, new FakeWebTextLoader(json), CreateSelector(1, int.MaxValue));

            string text = provider.GetTextAsync().GetAwaiter().GetResult();

            Assert.That(KKnightsPostParser.Parse(json).Paragraphs, Does.Contain(text));
        }
    }
}
