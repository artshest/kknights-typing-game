using System;
using System.Linq;
using KK.Infrastructure.Texts.KKnights;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    public class KKnightsPostParserTests
    {
        [TestCase(null)]
        [TestCase("")]
        public void Parse_EmptyJson_Throws(string json)
        {
            Assert.Throws<ArgumentException>(() => KKnightsPostParser.Parse(json));
        }

        [Test]
        public void Parse_NoText_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => KKnightsPostParser.Parse("{ 'slug': 'empty', 'content': [] }"));
        }

        [Test]
        public void Parse_BlocksFixture_ReadsTitleAndSlug()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Blocks));

            Assert.That(post.Title, Is.EqualTo("Старое & Доброе фэнтези"));
            Assert.That(post.Slug, Is.EqualTo("1508-staroe-dobroe-fentezi"));
        }

        [Test]
        public void Parse_BlocksFixture_SplitsHtmlIntoParagraphs()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Blocks));

            Assert.That(post.Paragraphs[0], Does.StartWith("Для участия в активности"));
            Assert.That(post.Paragraphs[0], Does.EndWith("был открыт по-новому."));
            Assert.That(post.Paragraphs, Does.Contain("Nox"));
            Assert.That(post.Paragraphs, Does.Contain("Dungeon siege"));
        }

        [Test]
        public void Parse_BlocksFixture_LeavesNoHtml()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Blocks));

            Assert.That(post.Paragraphs.Where(p => p.Contains('<') || p.Contains('>')), Is.Empty);
        }

        [Test]
        public void Parse_BlocksFixture_SkipsGalleriesAndEmbeds()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Blocks));

            Assert.That(post.Text, Does.Not.Contain("Ностальгией пахнет"));
            Assert.That(post.Text, Does.Not.Contain("youtu"));
        }

        [Test]
        public void Parse_DocumentFixture_ReadsTitleAndSlug()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Document));

            Assert.That(post.Title, Is.EqualTo("Valor Mortis"));
            Assert.That(post.Slug, Is.EqualTo("5235-valor-mortis"));
        }

        [Test]
        public void Parse_DocumentFixture_SplitsNodesIntoParagraphs()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Document));

            Assert.That(post.Paragraphs[0], Does.StartWith("Соулс-лайк от первого лица"));
            Assert.That(post.Paragraphs[1], Is.EqualTo("Сюжет"));
            Assert.That(post.Paragraphs[2], Does.StartWith("На дворе 19 век."));
        }

        [Test]
        public void Parse_DocumentFixture_SplitsOnHardBreak()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Document));

            int last = post.Paragraphs.Count - 1;
            Assert.That(post.Paragraphs[last], Does.StartWith("P.S. Хоспаде"));
            Assert.That(post.Paragraphs[last - 1], Does.EndWith("субъективная оценка - 9/10"));
        }

        [Test]
        public void Parse_DocumentFixture_CollapsesSpaces()
        {
            var post = KKnightsPostParser.Parse(KKnightsFixtures.Load(KKnightsFixtures.Document));

            Assert.That(post.Text, Does.Contain("наблюдает тотальный"));
            Assert.That(post.Paragraphs.Where(p => p.Contains("  ") || p != p.Trim()), Is.Empty);
        }

        // Newtonsoft допускает одинарные кавычки, поэтому JSON в тестах не нужно экранировать
        [Test]
        public void Parse_BlockWithoutContent_IsSkipped()
        {
            const string json = "{ 'content': [ { 'json': { 'url': 'https://youtu.be/x' } }, { 'json': { 'content': '<p>a</p>' } } ] }";

            var post = KKnightsPostParser.Parse(json);

            Assert.That(post.Paragraphs, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void Parse_HtmlEntitiesAndLineBreaks_DecodesAndSplits()
        {
            const string json = "{ 'content': [ { 'json': { 'content': '<p>a &amp; b&nbsp;c<br>d</p><p>e</p>' } } ] }";

            var post = KKnightsPostParser.Parse(json);

            Assert.That(post.Paragraphs, Is.EqualTo(new[] { "a & b c", "d", "e" }));
        }

        [Test]
        public void Parse_TextNodesWithMarks_JoinsIntoOneParagraph()
        {
            const string json = "{ 'document': { 'doc': { 'type': 'doc', 'content': [ { 'type': 'paragraph', 'content': [ " +
                                "{ 'type': 'text', 'text': 'обычный ' }, { 'type': 'text', 'text': 'жирный', 'marks': [ { 'type': 'bold' } ] } ] } ] } } }";

            var post = KKnightsPostParser.Parse(json);

            Assert.That(post.Paragraphs, Is.EqualTo(new[] { "обычный жирный" }));
        }

        [Test]
        public void Parse_NestedList_SplitsItems()
        {
            const string json = "{ 'document': { 'doc': { 'type': 'doc', 'content': [ { 'type': 'bulletList', 'content': [ " +
                                "{ 'type': 'listItem', 'content': [ { 'type': 'paragraph', 'content': [ { 'type': 'text', 'text': 'раз' } ] } ] }, " +
                                "{ 'type': 'listItem', 'content': [ { 'type': 'paragraph', 'content': [ { 'type': 'text', 'text': 'два' } ] } ] } ] } ] } } }";

            var post = KKnightsPostParser.Parse(json);

            Assert.That(post.Paragraphs, Is.EqualTo(new[] { "раз", "два" }));
        }
    }
}
