using System;
using NUnit.Framework;

namespace KK.Build.Editor.Tests
{
    public class ChangelogReleaserTests
    {
        private static readonly BuildVersion Version = new BuildVersion(0, 1, 0);
        private static readonly DateTime Date = new DateTime(2026, 11, 8);

        private const string Changelog =
            "# Changelog\n" +
            "\n" +
            "## [Unreleased]\n" +
            "\n" +
            "### Added\n" +
            "- Режим тренировки\n" +
            "\n" +
            "## [0.0.1] - 2026-11-01\n" +
            "- Первая сборка\n";

        [Test]
        public void Release_RenamesUnreleasedAndAddsEmptyOne()
        {
            var expected =
                "# Changelog\n" +
                "\n" +
                "## [Unreleased]\n" +
                "\n" +
                "## [0.1.0] - 2026-11-08\n" +
                "\n" +
                "### Added\n" +
                "- Режим тренировки\n" +
                "\n" +
                "## [0.0.1] - 2026-11-01\n" +
                "- Первая сборка\n";

            Assert.That(ChangelogReleaser.Release(Changelog, Version, Date), Is.EqualTo(expected));
        }

        [Test]
        public void Release_KeepsWindowsLineEndings()
        {
            var text = Changelog.Replace("\n", "\r\n");

            var result = ChangelogReleaser.Release(text, Version, Date);

            Assert.That(result.Replace("\r\n", string.Empty), Does.Not.Contain("\n"));
            Assert.That(result, Does.Contain("## [Unreleased]\r\n\r\n## [0.1.0] - 2026-11-08\r\n"));
        }

        [Test]
        public void Release_WithoutUnreleased_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ChangelogReleaser.Release("# Changelog\n", Version, Date));
        }

        [Test]
        public void Release_VersionAlreadyReleased_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ChangelogReleaser.Release(Changelog, new BuildVersion(0, 0, 1), Date));
        }

        [Test]
        public void IsUnreleasedEmpty_WithItems_ReturnsFalse()
        {
            Assert.That(ChangelogReleaser.IsUnreleasedEmpty(Changelog), Is.False);
        }

        [Test]
        public void IsUnreleasedEmpty_OnlyWhitespaceBeforeNextSection_ReturnsTrue()
        {
            var text = "# Changelog\n\n## [Unreleased]\n\n  \n## [0.0.1] - 2026-11-01\n- Первая сборка\n";

            Assert.That(ChangelogReleaser.IsUnreleasedEmpty(text), Is.True);
        }

        [Test]
        public void IsUnreleasedEmpty_LastSectionWithoutItems_ReturnsTrue()
        {
            Assert.That(ChangelogReleaser.IsUnreleasedEmpty("# Changelog\n\n## [Unreleased]\n"), Is.True);
        }

        [Test]
        public void HasUnreleasedSection_WithoutSection_ReturnsFalse()
        {
            Assert.That(ChangelogReleaser.HasUnreleasedSection("# Changelog\n## [0.0.1]\n"), Is.False);
        }
    }
}
