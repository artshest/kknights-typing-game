using System;
using System.Collections.Generic;
using KK.Core.Texts;
using NUnit.Framework;

namespace KK.Core.Tests.Texts
{
    public class TextFragmentSelectorTests
    {
        private const int Min = 10;
        private const int Max = 20;

        // Random.Next виртуальный: подменяем его, чтобы задать стартовый абзац явно
        private class FixedRandom : Random
        {
            private readonly int _value;

            public FixedRandom(int value) => _value = value;

            public override int Next(int maxValue) => _value;
        }

        private static string P(char letter, int length) => new string(letter, length);

        private static TextFragmentSelector CreateSelector(Random random) =>
            new TextFragmentSelector(new TextFragmentOptions { MinLength = Min, MaxLength = Max }, random);

        private static string Select(int start, params string[] paragraphs) =>
            CreateSelector(new FixedRandom(start)).Select(paragraphs);

        [Test]
        public void Select_StartParagraphLongEnough_ReturnsItAlone()
        {
            Assert.That(Select(0, P('а', 12), P('б', 5)), Is.EqualTo(P('а', 12)));
        }

        [Test]
        public void Select_StartParagraphShort_AppendsNextUntilMinLength()
        {
            string result = Select(1, P('а', 5), P('б', 5), P('в', 5), P('г', 5));

            Assert.That(result, Is.EqualTo(P('б', 5) + " " + P('в', 5)));
        }

        [Test]
        public void Select_LastParagraphShort_PrependsPrevious()
        {
            string result = Select(1, P('а', 5), P('б', 5));

            Assert.That(result, Is.EqualTo(P('а', 5) + " " + P('б', 5)));
        }

        [Test]
        public void Select_NextParagraphExceedsMax_PrependsPrevious()
        {
            string result = Select(1, P('а', 5), P('б', 5), P('в', 15));

            Assert.That(result, Is.EqualTo(P('а', 5) + " " + P('б', 5)));
        }

        [Test]
        public void Select_NoNeighbourFits_ReturnsShorterThanMin()
        {
            Assert.That(Select(1, P('а', 15), P('б', 5), P('в', 15)), Is.EqualTo(P('б', 5)));
        }

        [Test]
        public void Select_ArticleShorterThanMin_ReturnsWholeArticle()
        {
            Assert.That(Select(0, "ааа", "ббб"), Is.EqualTo("ааа ббб"));
        }

        [Test]
        public void Select_ParagraphLongerThanMax_NeverStartsFromIt()
        {
            // Кандидат на старт только второй абзац, поэтому индекс 0 от Random указывает на него
            Assert.That(Select(0, P('а', 30), P('б', 12)), Is.EqualTo(P('б', 12)));
        }

        [Test]
        public void Select_AllParagraphsLongerThanMax_ReturnsShortest()
        {
            Assert.That(Select(0, P('а', 30), P('б', 25), P('в', 40)), Is.EqualTo(P('б', 25)));
        }

        [Test]
        public void Select_EmptyParagraphs_AreSkipped()
        {
            string result = Select(0, P('а', 5), "", "   ", P('б', 5));

            Assert.That(result, Is.EqualTo(P('а', 5) + " " + P('б', 5)));
        }

        [Test]
        public void Select_ManyCalls_NeverLongerThanMax()
        {
            var random = new Random(0);
            var selector = CreateSelector(random);
            var tooLong = new List<string>();

            for (int i = 0; i < 200; i++)
            {
                var paragraphs = new string[random.Next(1, 8)];
                for (int p = 0; p < paragraphs.Length; p++)
                    paragraphs[p] = P('а', random.Next(1, Max + 1));

                string result = selector.Select(paragraphs);
                if (result.Length > Max)
                    tooLong.Add($"{string.Join("|", paragraphs)} -> {result.Length}");
            }

            Assert.That(tooLong, Is.Empty);
        }

        [Test]
        public void Select_ManyCalls_StartsFromEveryParagraph()
        {
            string[] paragraphs = { P('а', 12), P('б', 12), P('в', 12) };
            var selector = CreateSelector(new Random(0));
            var seen = new HashSet<string>();

            for (int i = 0; i < 100; i++)
                seen.Add(selector.Select(paragraphs));

            Assert.That(seen, Is.EquivalentTo(paragraphs));
        }

        [Test]
        public void Select_NoTextParagraphs_Throws()
        {
            Assert.Throws<ArgumentException>(() => Select(0, "", "  "));
        }

        [Test]
        public void Select_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CreateSelector(new Random(0)).Select(null));
        }

        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(10, 9)]
        public void Constructor_InvalidBounds_Throws(int min, int max)
        {
            var options = new TextFragmentOptions { MinLength = min, MaxLength = max };

            Assert.Throws<ArgumentException>(() => new TextFragmentSelector(options, new Random(0)));
        }

        [Test]
        public void Constructor_EqualBounds_DoesNotThrow()
        {
            var options = new TextFragmentOptions { MinLength = 10, MaxLength = 10 };

            Assert.DoesNotThrow(() => new TextFragmentSelector(options, new Random(0)));
        }

        [Test]
        public void Constructor_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TextFragmentSelector(null, new Random(0)));
        }

        [Test]
        public void Constructor_NullRandom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TextFragmentSelector(new TextFragmentOptions(), null));
        }
    }
}
