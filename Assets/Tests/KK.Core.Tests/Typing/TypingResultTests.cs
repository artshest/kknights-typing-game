using System;
using KK.Core.Typing;
using NUnit.Framework;

namespace KK.Core.Tests.Typing
{
    public class TypingResultTests
    {
        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            TypingResult a = Create(mistakes: 1);
            TypingResult b = Create(mistakes: 1);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            TypingResult a = Create(mistakes: 1);
            TypingResult b = Create(mistakes: 2);

            Assert.That(a.Equals(b), Is.False);
        }

        private static TypingResult Create(int mistakes)
        {
            return new TypingResult(120, 0.9, mistakes, 9, TimeSpan.FromSeconds(5));
        }
    }
}
