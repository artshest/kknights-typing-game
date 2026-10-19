using System;
using KK.Core.Typing;
using NUnit.Framework;

namespace KK.Core.Tests.Typing
{
    public class KeystrokeTests
    {
        [TestCase('a', 'a', true)]
        [TestCase('a', 'A', false)]
        [TestCase('е', 'ё', false)]
        public void IsCorrect_ComparesStrictly(char expected, char typed, bool isCorrect)
        {
            var keystroke = new Keystroke(0, expected, typed, TimeSpan.Zero);

            Assert.That(keystroke.IsCorrect, Is.EqualTo(isCorrect));
        }
    }
}
