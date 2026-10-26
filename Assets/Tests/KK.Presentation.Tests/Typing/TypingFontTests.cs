using System.Collections.Generic;
using KK.Core.Texts;
using NUnit.Framework;
using TMPro;
using UnityEditor;

namespace KK.Presentation.Tests.Typing
{
    // Атлас статический: символ, которого в нём нет, игрок увидит как пустой квадрат
    public class TypingFontTests
    {
        private const string FontAssetPath = "Assets/Fonts/NotoSans/NotoSans-Regular SDF.asset";

        [Test]
        public void FontAsset_ContainsAllCharsAllowedByNormalizer()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Assert.That(font, Is.Not.Null, $"Шрифт {FontAssetPath} не найден");

            var missing = new List<char>();
            foreach (char ch in AllowedChars())
            {
                if (!font.HasCharacter(ch))
                    missing.Add(ch);
            }

            Assert.That(missing, Is.Empty);
        }

        [Test]
        public void FontAsset_IsDefaultInTmpSettings()
        {
            Assert.That(AssetDatabase.GetAssetPath(TMP_Settings.defaultFontAsset), Is.EqualTo(FontAssetPath));
        }

        // Берём из самого нормализатора: если белый список расширят, тест потребует обновить атлас
        private static IEnumerable<char> AllowedChars()
        {
            var options = new TextNormalizationOptions { ReplaceYo = false };

            // Одиночный пробел нормализатор обрезает по краям, хотя внутри текста он допустим
            yield return ' ';
            for (int code = char.MinValue; code <= char.MaxValue; code++)
            {
                // Половина суррогатной пары — некорректная строка, string.Normalize на ней бросает исключение
                if (char.IsSurrogate((char)code))
                    continue;

                string single = ((char)code).ToString();
                if (TextNormalizer.Normalize(single, options) == single)
                    yield return (char)code;
            }
        }
    }
}
