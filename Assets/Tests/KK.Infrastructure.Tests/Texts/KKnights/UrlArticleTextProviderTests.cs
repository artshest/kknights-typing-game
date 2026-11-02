using System;
using KK.Infrastructure.Tests.Fakes;
using KK.Infrastructure.Texts.KKnights;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    public class UrlArticleTextProviderTests
    {
        private const string ArticleLink = "https://kknights.com/posts/5235-valor-mortis";

        [Test]
        public void Constructor_NotArticleLink_Throws()
        {
            var loader = new FakeWebTextLoader("{}");

            Assert.Throws<ArgumentException>(() => new UrlArticleTextProvider("https://example.com/", loader));
        }

        [Test]
        public void GetTextAsync_ArticleLink_RequestsApiUrl()
        {
            var loader = new FakeWebTextLoader(KKnightsFixtures.Load(KKnightsFixtures.Document));
            var provider = new UrlArticleTextProvider(ArticleLink, loader);

            provider.GetTextAsync().GetAwaiter().GetResult();

            Assert.That(loader.RequestedUrls, Is.EqualTo(new[] { "https://kknights.com/api/v1/post/5235-valor-mortis/" }));
        }

        [Test]
        public void GetTextAsync_ReturnsFirstParagraph()
        {
            string json = KKnightsFixtures.Load(KKnightsFixtures.Document);
            var provider = new UrlArticleTextProvider(ArticleLink, new FakeWebTextLoader(json));

            string text = provider.GetTextAsync().GetAwaiter().GetResult();

            Assert.That(text, Is.EqualTo(KKnightsPostParser.Parse(json).Paragraphs[0]));
        }
    }
}
