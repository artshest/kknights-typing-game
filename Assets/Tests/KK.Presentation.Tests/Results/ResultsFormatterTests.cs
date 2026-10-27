using System;
using KK.Presentation.Results;
using NUnit.Framework;

namespace KK.Presentation.Tests.Results
{
    public class ResultsFormatterTests
    {
        [TestCase(312.4, "312 зн/мин")]
        [TestCase(0, "0 зн/мин")]
        [TestCase(1234.6, "1235 зн/мин")]
        public void FormatSpeed_RoundsToWholeNumber(double speed, string expected)
        {
            Assert.That(ResultsFormatter.FormatSpeed(speed), Is.EqualTo(expected));
        }

        [TestCase(0.9654, "96,5 %")]
        [TestCase(1, "100,0 %")]
        [TestCase(0, "0,0 %")]
        public void FormatAccuracy_ConvertsFractionToPercentWithComma(double accuracy, string expected)
        {
            Assert.That(ResultsFormatter.FormatAccuracy(accuracy), Is.EqualTo(expected));
        }

        [TestCase(0, "0:00")]
        [TestCase(5, "0:05")]
        [TestCase(65, "1:05")]
        [TestCase(59.6, "1:00")]
        [TestCase(3725, "62:05")]
        public void FormatDuration_ShowsMinutesAndSeconds(double seconds, string expected)
        {
            Assert.That(ResultsFormatter.FormatDuration(TimeSpan.FromSeconds(seconds)), Is.EqualTo(expected));
        }

        [Test]
        public void FormatMistakes_ShowsNumber()
        {
            Assert.That(ResultsFormatter.FormatMistakes(7), Is.EqualTo("7"));
        }
    }
}
