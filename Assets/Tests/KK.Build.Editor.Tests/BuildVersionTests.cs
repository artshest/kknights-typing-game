using System;
using NUnit.Framework;

namespace KK.Build.Editor.Tests
{
    public class BuildVersionTests
    {
        [TestCase("0.0.1", 0, 0, 1)]
        [TestCase("0.1.0", 0, 1, 0)]
        [TestCase("12.345.6789", 12, 345, 6789)]
        public void TryParse_ValidVersion_ReturnsParts(string text, int major, int minor, int patch)
        {
            Assert.That(BuildVersion.TryParse(text, out var version), Is.True);
            Assert.That(version, Is.EqualTo(new BuildVersion(major, minor, patch)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("1")]
        [TestCase("1.2")]
        [TestCase("1.2.3.4")]
        [TestCase("1..3")]
        [TestCase("01.2.3")]
        [TestCase("1.02.3")]
        [TestCase("1.2.03")]
        [TestCase("-1.2.3")]
        [TestCase("+1.2.3")]
        [TestCase(" 1.2.3")]
        [TestCase("1.2.3 ")]
        [TestCase("1.2.3-beta")]
        [TestCase("1.2.3+abc")]
        [TestCase("v1.2.3")]
        [TestCase("1.2.99999999999")]
        public void TryParse_InvalidVersion_ReturnsFalse(string text)
        {
            Assert.That(BuildVersion.TryParse(text, out _), Is.False);
        }

        [Test]
        public void Parse_InvalidVersion_Throws()
        {
            Assert.Throws<FormatException>(() => BuildVersion.Parse("1.2"));
        }

        [Test]
        public void NextPatch_IncrementsPatchNumerically()
        {
            Assert.That(BuildVersion.Parse("0.0.9").NextPatch().ToString(), Is.EqualTo("0.0.10"));
        }

        [Test]
        public void NextMinor_ResetsPatch()
        {
            Assert.That(BuildVersion.Parse("0.1.7").NextMinor().ToString(), Is.EqualTo("0.2.0"));
        }

        [Test]
        public void NextMajor_ResetsMinorAndPatch()
        {
            Assert.That(BuildVersion.Parse("0.9.3").NextMajor().ToString(), Is.EqualTo("1.0.0"));
        }

        [Test]
        public void Constructor_NegativePart_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildVersion(0, -1, 0));
        }

        [Test]
        public void ToString_RoundTripsThroughParse()
        {
            var version = new BuildVersion(1, 20, 300);

            Assert.That(BuildVersion.Parse(version.ToString()), Is.EqualTo(version));
        }
    }
}
