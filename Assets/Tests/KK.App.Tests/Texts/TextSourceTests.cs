using System;
using System.Collections.Generic;
using KK.App.Texts;
using NUnit.Framework;

namespace KK.App.Tests.Texts
{
    public class TextSourceTests
    {
        private const string Link = "https://kknights.com/posts/test";

        [Test]
        public void Library_HasKindAndNoLink()
        {
            TextSource source = TextSource.Library();

            Assert.That(source.Kind, Is.EqualTo(TextSourceKind.Library));
            Assert.That(source.ArticleLink, Is.Null);
        }

        [Test]
        public void Article_KeepsLinkAsIs()
        {
            // Ссылку проверяет и чистит валидатор, а не TextSource
            TextSource source = TextSource.Article("  " + Link);

            Assert.That(source.Kind, Is.EqualTo(TextSourceKind.Article));
            Assert.That(source.ArticleLink, Is.EqualTo("  " + Link));
        }

        [Test]
        public void Article_Null_ThrowsWithParamName()
        {
            var e = Assert.Throws<ArgumentNullException>(() => TextSource.Article(null));

            Assert.That(e.ParamName, Is.EqualTo("articleLink"));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\n\t")]
        public void Article_Blank_ThrowsWithParamName(string link)
        {
            var e = Assert.Throws<ArgumentException>(() => TextSource.Article(link));

            Assert.That(e.ParamName, Is.EqualTo("articleLink"));
        }

        [Test]
        public void TwoLibraries_AreEqualInEveryWay()
        {
            AssertEqual(TextSource.Library(), TextSource.Library());
        }

        [Test]
        public void ArticlesWithSameLink_AreEqualInEveryWay()
        {
            AssertEqual(TextSource.Article(Link), TextSource.Article(Link));
        }

        [Test]
        public void ArticlesWithDifferentLinks_AreNotEqual()
        {
            AssertNotEqual(TextSource.Article(Link), TextSource.Article(Link + "2"));
        }

        [Test]
        public void LibraryAndArticle_AreNotEqual()
        {
            AssertNotEqual(TextSource.Library(), TextSource.Article(Link));
        }

        [Test]
        public void ComparisonWithNull_IsFalseAndDoesNotThrow()
        {
            TextSource source = TextSource.Library();
            TextSource none = null;

            Assert.That(source.Equals(none), Is.False);
            Assert.That(source.Equals((object)null), Is.False);
            Assert.That(source == null, Is.False);
            Assert.That(null == source, Is.False);
            Assert.That(source != null, Is.True);
            Assert.That(none == null, Is.True);
        }

        [Test]
        public void ComparisonWithOtherType_IsFalse()
        {
            Assert.That(TextSource.Article(Link).Equals(Link), Is.False);
        }

        [Test]
        public void EqualSource_FindsKeyInDictionary()
        {
            // Без согласованного GetHashCode словарь ищет равный ключ не в той «корзине» и не находит
            var providers = new Dictionary<TextSource, string> { [TextSource.Article(Link)] = "провайдер" };

            Assert.That(providers.ContainsKey(TextSource.Article(Link)), Is.True);
        }

        private static void AssertEqual(TextSource a, TextSource b)
        {
            Assert.That(a.Equals(b), Is.True, "Equals(TextSource)");
            Assert.That(b.Equals(a), Is.True, "Equals(TextSource) несимметричен");
            Assert.That(a.Equals((object)b), Is.True, "Equals(object)");
            Assert.That(a == b, Is.True, "==");
            Assert.That(a != b, Is.False, "!=");
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()), "У равных объектов хеш обязан совпадать");
        }

        private static void AssertNotEqual(TextSource a, TextSource b)
        {
            Assert.That(a.Equals(b), Is.False, "Equals(TextSource)");
            Assert.That(b.Equals(a), Is.False, "Equals(TextSource) несимметричен");
            Assert.That(a.Equals((object)b), Is.False, "Equals(object)");
            Assert.That(a == b, Is.False, "==");
            Assert.That(a != b, Is.True, "!=");
        }
    }
}
