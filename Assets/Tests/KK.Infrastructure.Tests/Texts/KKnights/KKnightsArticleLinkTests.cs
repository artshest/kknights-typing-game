using KK.Infrastructure.Texts.KKnights;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    public class KKnightsArticleLinkTests
    {
        private const string Slug = "5226-vpechatleniia-ot-demo-versii-sluchainaia-podborka-no4";

        [TestCase("https://kknights.com/posts/" + Slug)]
        [TestCase("https://kknights.com/posts/" + Slug + "/")]
        [TestCase("http://kknights.com/posts/" + Slug)]
        [TestCase("https://www.kknights.com/posts/" + Slug)]
        [TestCase("https://KKnights.com/posts/" + Slug)]
        [TestCase("kknights.com/posts/" + Slug)]
        [TestCase("  https://kknights.com/posts/" + Slug + " \n")]
        [TestCase("https://kknights.com/posts/" + Slug + "?utm_source=telegram")]
        [TestCase("https://kknights.com/posts/" + Slug + "#comments")]
        [TestCase("https://kknights.com/api/v1/post/" + Slug + "/")]
        public void TryParseSlug_ArticleLink_ReturnsSlug(string link)
        {
            bool parsed = KKnightsArticleLink.TryParseSlug(link, out string slug);

            Assert.That(parsed, Is.True);
            Assert.That(slug, Is.EqualTo(Slug));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("просто текст")]
        [TestCase("https://kknights.com/")]
        [TestCase("https://kknights.com/posts/")]
        [TestCase("https://kknights.com/posts/" + Slug + "/comments")]
        [TestCase("https://kknights.com/users/" + Slug)]
        [TestCase("https://example.com/posts/" + Slug)]
        [TestCase("https://kknights.com.example.com/posts/" + Slug)]
        [TestCase("ftp://kknights.com/posts/" + Slug)]
        [TestCase("https://kknights.com/posts/valor%2Fmortis")]
        [TestCase("https://kknights.com/posts/ёлка")]
        public void TryParseSlug_NotArticleLink_ReturnsFalse(string link)
        {
            bool parsed = KKnightsArticleLink.TryParseSlug(link, out string slug);

            Assert.That(parsed, Is.False);
            Assert.That(slug, Is.Null);
        }

        [Test]
        public void ToApiUrl_BuildsPostEndpoint()
        {
            Assert.That(KKnightsArticleLink.ToApiUrl(Slug), Is.EqualTo("https://kknights.com/api/v1/post/" + Slug + "/"));
        }
    }
}
