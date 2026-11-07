using KK.Core.Texts;
using KK.Presentation.Menu;
using NUnit.Framework;

namespace KK.Presentation.Tests.Menu
{
    public class VersionFormatterTests
    {
        [Test]
        public void Format_Release_HidesCommit()
        {
            Assert.That(VersionFormatter.Format("0.1.0", "3e48c23", showCommit: false), Is.EqualTo("v0.1.0"));
        }

        [Test]
        public void Format_Development_AppendsCommit()
        {
            Assert.That(VersionFormatter.Format("0.1.0", "3e48c23", showCommit: true), Is.EqualTo("v0.1.0+3e48c23"));
        }

        [Test]
        public void Format_CommitWithLineBreak_IsTrimmed()
        {
            Assert.That(VersionFormatter.Format("0.1.0", "3e48c23\r\n", showCommit: true), Is.EqualTo("v0.1.0+3e48c23"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  \n")]
        public void Format_NoCommit_ShowsOnlyVersion(string commit)
        {
            Assert.That(VersionFormatter.Format("0.1.0", commit, showCommit: true), Is.EqualTo("v0.1.0"));
        }

        [Test]
        public void Format_UsesOnlyFontCharacters()
        {
            var options = new TextNormalizationOptions { ReplaceYo = false };
            var text = VersionFormatter.Format("10.20.30", "0123456789abcdef", showCommit: true);

            Assert.That(TextNormalizer.Normalize(text, options), Is.EqualTo(text));
        }
    }
}
