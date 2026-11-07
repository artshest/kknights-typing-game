using KK.Core.Texts;
using KK.Presentation.Menu;
using NUnit.Framework;

namespace KK.Presentation.Tests.Menu
{
    public class MenuTextsTests
    {
        [Test]
        public void Texts_UseOnlyFontCharacters()
        {
            // Нормализатор не трогает строку, только если все её символы есть в белом списке, а значит и в атласе шрифта
            var options = new TextNormalizationOptions { ReplaceYo = false };

            Assert.That(TextNormalizer.Normalize(MenuTexts.InvalidLink, options), Is.EqualTo(MenuTexts.InvalidLink));
        }
    }
}
