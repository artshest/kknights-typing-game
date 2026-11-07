using System;
using KK.Core.Texts;
using KK.Infrastructure.Tests.Fakes;
using KK.Infrastructure.Texts.KKnights;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    // Сами правила ссылок проверяет KKnightsArticleLinkTests; здесь — что валидатор их использует
    public class KKnightsArticleLinkValidatorTests
    {
        private readonly KKnightsArticleLinkValidator _validator = new();

        [TestCase("https://kknights.com/posts/5235-valor-mortis")]
        [TestCase("  kknights.com/posts/5235-valor-mortis ")]
        public void ArticleLink_IsValid(string link)
        {
            Assert.That(_validator.IsValid(link), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("просто текст")]
        [TestCase("https://example.com/posts/5235-valor-mortis")]
        public void NotArticleLink_IsInvalid(string link)
        {
            Assert.That(_validator.IsValid(link), Is.False);
        }

        [Test]
        public void ValidLink_IsAcceptedByProvider()
        {
            // Главное обещание валидатора: прошедшая проверку ссылка не уронит конструктор провайдера
            const string link = "  kknights.com/posts/5235-valor-mortis ";
            Assume.That(_validator.IsValid(link), Is.True);

            Assert.DoesNotThrow(() => new UrlArticleTextProvider(link, new FakeWebTextLoader("{}"),
                new TextFragmentSelector(new TextFragmentOptions(), new Random())));
        }
    }
}
