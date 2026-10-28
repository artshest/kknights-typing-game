using System.Collections.Generic;
using KK.Core.Texts;
using KK.Infrastructure.Texts;
using NUnit.Framework;
using UnityEditor;

namespace KK.Infrastructure.Tests.Texts
{
    // Проверяем данные против правил кода: иначе нормализатор молча выкинет символ,
    // и игрок наберёт не тот текст, что написал автор
    public class TextLibraryAssetTests
    {
        private const string LibraryPath = "Assets/Texts/TextLibrary.asset";

        private TextLibrary _library;

        [SetUp]
        public void SetUp()
        {
            _library = AssetDatabase.LoadAssetAtPath<TextLibrary>(LibraryPath);
            Assert.That(_library, Is.Not.Null, $"Библиотека {LibraryPath} не найдена");
        }

        [Test]
        public void Library_HasTexts()
        {
            Assert.That(_library.Texts, Is.Not.Empty);
        }

        [Test]
        public void Texts_AreNotEmpty()
        {
            // Строку из одних пробелов нормализатор обрежет до пустой — игрок получит пустой раунд
            var empty = new List<int>();
            for (int i = 0; i < _library.Texts.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(_library.Texts[i]))
                    empty.Add(i);
            }

            Assert.That(empty, Is.Empty, "Номера пустых текстов");
        }

        [Test]
        public void Texts_LoseNoCharsInNormalization()
        {
            // «ё» в библиотеке допустима: замену на «е» включает настройка игрока, а не автор текста
            var options = new TextNormalizationOptions { ReplaceYo = false };

            // Замены (« -> ", — -> -) штатные и смысл не меняют, поэтому сравниваем не тексты целиком,
            // а каждый символ отдельно: потерян тот, от которого после нормализации ничего не осталось.
            // Собираем все потери, чтобы автор увидел их разом, а не по одному за запуск
            var lost = new List<string>();
            for (int i = 0; i < _library.Texts.Count; i++)
            {
                string text = _library.Texts[i];
                for (int pos = 0; pos < text.Length; pos++)
                {
                    // Пробелы и переводы строк нормализатор законно обрезает по краям и схлопывает
                    if (char.IsWhiteSpace(text[pos]))
                        continue;

                    // Эмодзи — суррогатная пара. В белом списке таких символов нет, а на половине пары
                    // string.Normalize бросает исключение, поэтому суррогаты считаем потерянными без вызова нормализатора
                    if (char.IsSurrogate(text[pos]))
                    {
                        lost.Add($"#{i}, позиция {pos}: U+{(int)text[pos]:X4} (часть эмодзи или символа вне BMP)");
                        continue;
                    }

                    string symbol = text[pos].ToString();
                    if (TextNormalizer.Normalize(symbol, options).Length == 0)
                        lost.Add($"#{i}, позиция {pos}: «{symbol}» U+{(int)text[pos]:X4}");
                }
            }

            Assert.That(lost, Is.Empty, "Символы, которые нормализатор удалит");
        }
    }
}
