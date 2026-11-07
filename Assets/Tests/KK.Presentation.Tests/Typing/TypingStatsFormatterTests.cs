using System;
using KK.Presentation.Typing;
using NUnit.Framework;

namespace KK.Presentation.Tests.Typing
{
    public class TypingStatsFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(213.6, "214")]
        public void FormatSpeed_RoundsToWholeNumber(double speed, string expected)
        {
            Assert.That(TypingStatsFormatter.FormatSpeed(speed), Is.EqualTo(expected));
        }

        [TestCase(1, "100%")]
        [TestCase(0.956, "96%")]
        [TestCase(0, "0%")]
        public void FormatAccuracy_ShowsWholePercent(double accuracy, string expected)
        {
            Assert.That(TypingStatsFormatter.FormatAccuracy(accuracy), Is.EqualTo(expected));
        }

        [Test]
        public void FormatSeconds_DropsFraction()
        {
            Assert.That(TypingStatsFormatter.FormatSeconds(TimeSpan.FromSeconds(84.9)), Is.EqualTo("84"));
        }

        [Test]
        public void FormatTypedCount_ShowsTypedOfTotal()
        {
            Assert.That(TypingStatsFormatter.FormatTypedCount(77, 117), Is.EqualTo("Символы: 77 / 117"));
        }
    }
}
