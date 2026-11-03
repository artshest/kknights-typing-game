using System;
using System.Collections.Generic;
using System.Linq;
using KK.App.Texts;
using KK.Core.Texts;
using KK.Presentation.Loading;
using NUnit.Framework;

namespace KK.Presentation.Tests.Loading
{
    public class LoadErrorFormatterTests
    {
        // Перебор через Enum.GetValues: новая причина без своего текста уронит тесты сама
        private static IEnumerable<TextLoadErrorReason> AllReasons =>
            Enum.GetValues(typeof(TextLoadErrorReason)).Cast<TextLoadErrorReason>();

        private static IEnumerable<string> AllTexts =>
            AllReasons.Select(LoadErrorFormatter.Format)
                .Append(LoadErrorFormatter.Loading)
                .Append(LoadErrorFormatter.Unexpected);

        [Test]
        public void Format_EveryReason_HasOwnText()
        {
            var withoutText = AllReasons
                .Where(reason => string.IsNullOrWhiteSpace(LoadErrorFormatter.Format(reason))
                                 || LoadErrorFormatter.Format(reason) == LoadErrorFormatter.Unexpected)
                .ToList();

            Assert.That(withoutText, Is.Empty, "Причины без своего текста");
        }

        [Test]
        public void Format_DifferentReasons_DifferentTexts()
        {
            var texts = AllReasons.Select(LoadErrorFormatter.Format).ToList();

            Assert.That(texts, Is.Unique);
        }

        [Test]
        public void Format_UnknownReason_ReturnsUnexpectedText()
        {
            Assert.That(LoadErrorFormatter.Format((TextLoadErrorReason)999), Is.EqualTo(LoadErrorFormatter.Unexpected));
        }

        [Test]
        public void Texts_UseOnlyFontCharacters()
        {
            // Нормализатор не трогает строку, только если все её символы есть в белом списке, а значит и в атласе шрифта
            var options = new TextNormalizationOptions { ReplaceYo = false };
            var changed = AllTexts.Where(text => TextNormalizer.Normalize(text, options) != text).ToList();

            Assert.That(changed, Is.Empty, "Тексты с символами, которых нет в шрифте");
        }
    }
}
