using System;
using KK.Core.Typing;
using NUnit.Framework;

namespace KK.Core.Tests.Typing
{
    public class StatsCalculatorTests
    {
        private const double Tolerance = 1e-9;

        [Test]
        public void Calculate_NullKeystrokes_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => StatsCalculator.Calculate(null, TimeSpan.Zero));
        }

        [Test]
        public void Calculate_NoKeystrokes_ReturnsZeroSpeedAndFullAccuracy()
        {
            TypingResult result = StatsCalculator.Calculate(Array.Empty<Keystroke>(), Seconds(10));

            Assert.That(result.CharactersPerMinute, Is.EqualTo(0));
            Assert.That(result.Accuracy, Is.EqualTo(1));
            Assert.That(result.Mistakes, Is.EqualTo(0));
            Assert.That(result.CorrectCount, Is.EqualTo(0));
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void Calculate_CorrectAndWrongPresses_CountsSpeedAndAccuracy()
        {
            Keystroke[] keystrokes = { Correct(10), Wrong(20), Correct(30), Correct(40) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(40));

            Assert.That(result.Duration, Is.EqualTo(Seconds(30)));
            Assert.That(result.CharactersPerMinute, Is.EqualTo(6).Within(Tolerance));
            Assert.That(result.Accuracy, Is.EqualTo(0.75).Within(Tolerance));
            Assert.That(result.Mistakes, Is.EqualTo(1));
            Assert.That(result.CorrectCount, Is.EqualTo(3));
        }

        [Test]
        public void Calculate_RepeatedWrongPresses_CountsEachAsMistake()
        {
            Keystroke[] keystrokes = { Wrong(1), Wrong(2), Wrong(3), Correct(4) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(4));

            Assert.That(result.Mistakes, Is.EqualTo(3));
            Assert.That(result.Accuracy, Is.EqualTo(0.25).Within(Tolerance));
        }

        [Test]
        public void Calculate_FirstPressAfterStart_CountsFromFirstPress()
        {
            Keystroke[] keystrokes = { Correct(5), Correct(35) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(65));

            Assert.That(result.Duration, Is.EqualTo(Seconds(60)));
            Assert.That(result.CharactersPerMinute, Is.EqualTo(2).Within(Tolerance));
        }

        [Test]
        public void Calculate_SingleKeystroke_ReturnsZeroSpeed()
        {
            Keystroke[] keystrokes = { Correct(1) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(1));

            Assert.That(result.Duration, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.CharactersPerMinute, Is.EqualTo(0));
        }

        [Test]
        public void Calculate_EndTimeBeforeFirstPress_ClampsDurationToZero()
        {
            Keystroke[] keystrokes = { Correct(5) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(1));

            Assert.That(result.Duration, Is.EqualTo(TimeSpan.Zero));
            Assert.That(result.CharactersPerMinute, Is.EqualTo(0));
        }

        [Test]
        public void Calculate_OnlyWrongPresses_ReturnsZeroSpeedAndAccuracy()
        {
            Keystroke[] keystrokes = { Wrong(0), Wrong(30) };

            TypingResult result = StatsCalculator.Calculate(keystrokes, Seconds(30));

            Assert.That(result.CharactersPerMinute, Is.EqualTo(0));
            Assert.That(result.Accuracy, Is.EqualTo(0));
            Assert.That(result.Mistakes, Is.EqualTo(2));
        }

        private static TimeSpan Seconds(double value) => TimeSpan.FromSeconds(value);

        private static Keystroke Correct(double seconds) => new Keystroke(0, 'a', 'a', Seconds(seconds));

        private static Keystroke Wrong(double seconds) => new Keystroke(0, 'a', 'x', Seconds(seconds));
    }
}
