using System;
using KK.Core.Texts;
using NUnit.Framework;

namespace KK.Core.Tests.Texts
{
    public class TextNormalizerTests
    {
        [TestCase("«Привет»", "\"Привет\"")]
        [TestCase("„Привет“", "\"Привет\"")]
        [TestCase("“Hello”", "\"Hello\"")]
        [TestCase("it’s", "it's")]
        [TestCase("‘a’", "'a'")]
        [TestCase("а — б", "а - б")]
        [TestCase("1–2", "1-2")]
        [TestCase("−5", "-5")]
        [TestCase("Ну…", "Ну...")]
        public void Normalize_Typography_ReplacesWithAscii(string input, string expected)
        {
            Assert.That(Normalize(input), Is.EqualTo(expected));
        }

        [TestCase("и\u0306", "й")]
        [TestCase("е\u0308", "е")]
        public void Normalize_DecomposedLetters_Composes(string input, string expected)
        {
            Assert.That(Normalize(input), Is.EqualTo(expected));
        }

        [TestCase("а\nб", "а б")]
        [TestCase("а\r\nб", "а б")]
        [TestCase("а\tб", "а б")]
        [TestCase("а б", "а б")]
        public void Normalize_WhitespaceAndLineBreaks_BecomeSpace(string input, string expected)
        {
            Assert.That(Normalize(input), Is.EqualTo(expected));
        }

        [TestCase("а   б", "а б")]
        [TestCase("  а  ", "а")]
        [TestCase("а\n\n\nб", "а б")]
        public void Normalize_RepeatedSpaces_CollapsesAndTrims(string input, string expected)
        {
            Assert.That(Normalize(input), Is.EqualTo(expected));
        }

        [TestCase("Привет 👋", "Привет")]
        [TestCase("а 👋 б", "а б")]
        [TestCase("中文abc", "abc")]
        [TestCase("👋", "")]
        public void Normalize_UntypeableChars_Removes(string input, string expected)
        {
            Assert.That(Normalize(input), Is.EqualTo(expected));
        }

        [TestCase("Hello, World! 123")]
        [TestCase("Съешь же ещё этих мягких французских булок")]
        [TestCase("№5 ~ {a} [b] @#$%^&*")]
        public void Normalize_TypeableText_KeepsAsIs(string input)
        {
            var options = new TextNormalizationOptions { ReplaceYo = false };

            Assert.That(TextNormalizer.Normalize(input, options), Is.EqualTo(input));
        }

        [Test]
        public void Normalize_ReplaceYoEnabled_ReplacesWithYe()
        {
            var options = new TextNormalizationOptions { ReplaceYo = true };

            Assert.That(TextNormalizer.Normalize("Ёлка и ёж", options), Is.EqualTo("Елка и еж"));
        }

        [Test]
        public void Normalize_ReplaceYoDisabled_KeepsYo()
        {
            var options = new TextNormalizationOptions { ReplaceYo = false };

            Assert.That(TextNormalizer.Normalize("Ёлка и ёж", options), Is.EqualTo("Ёлка и ёж"));
        }

        [Test]
        public void Normalize_NullText_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TextNormalizer.Normalize(null, TextNormalizationOptions.Default));
        }

        [Test]
        public void Normalize_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TextNormalizer.Normalize("а", null));
        }

        private static string Normalize(string text) => TextNormalizer.Normalize(text, TextNormalizationOptions.Default);
    }
}
