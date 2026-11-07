using KK.Presentation.Typing;
using NUnit.Framework;

namespace KK.Presentation.Tests.Typing
{
    public class CaretLayoutTests
    {
        [Test]
        public void Glyph_WithRightGap_CentersAdvanceOnGlyph()
        {
            // Ячейка 0..12, глиф 0..10: промежуток 2 только справа
            var (left, right) = CaretLayout.Glyph(origin: 0, xAdvance: 12, glyphLeft: 0, glyphRight: 10);

            Assert.That(left, Is.EqualTo(-1f));
            Assert.That(right, Is.EqualTo(11f));
        }

        [Test]
        public void Glyph_KeepsAdvanceWidth()
        {
            var (left, right) = CaretLayout.Glyph(origin: 100, xAdvance: 114, glyphLeft: 102, glyphRight: 110);

            Assert.That(right - left, Is.EqualTo(14f));
        }

        [Test]
        public void Space_BetweenLetters_CentersBetweenNeighbourGlyphs()
        {
            // Предыдущая буква кончается на 38 (её промежуток 38..40), пробел 40..48, следующая буква с 48
            var (left, right) = CaretLayout.Space(origin: 40, xAdvance: 48, previousGlyphRight: 38, nextGlyphLeft: 48);

            Assert.That(left, Is.EqualTo(39f));
            Assert.That(right, Is.EqualTo(47f));
        }

        [Test]
        public void Space_WithoutNeighbours_UsesAdvanceCell()
        {
            var (left, right) = CaretLayout.Space(origin: 40, xAdvance: 48, previousGlyphRight: null, nextGlyphLeft: null);

            Assert.That(left, Is.EqualTo(40f));
            Assert.That(right, Is.EqualTo(48f));
        }
    }
}
