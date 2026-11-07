using System.Linq;
using NUnit.Framework;

namespace KK.Build.Editor.Tests
{
    public class BuildCommandsTests
    {
        private static readonly string[] Args =
        {
            "Unity.exe", "-batchmode", "-executeMethod", "KK.Build.Editor.BuildCommands.Build",
            "-profile", "WebGL-Release, Windows-Release", "-noZip", "-version",
        };

        [Test]
        public void TryGetValue_ReturnsFollowingArgument()
        {
            Assert.That(BuildCommands.TryGetValue(Args, "-profile", out var value), Is.True);
            Assert.That(value, Is.EqualTo("WebGL-Release, Windows-Release"));
        }

        [Test]
        public void TryGetValue_NameIsLastArgument_ReturnsFalse()
        {
            Assert.That(BuildCommands.TryGetValue(Args, "-version", out _), Is.False);
        }

        [Test]
        public void TryGetValue_NextIsAnotherOption_ReturnsFalse()
        {
            Assert.That(BuildCommands.TryGetValue(new[] { "-version", "-noZip" }, "-version", out _), Is.False);
        }

        [Test]
        public void HasFlag_IsCaseInsensitive()
        {
            Assert.That(BuildCommands.HasFlag(Args, "-nozip"), Is.True);
            Assert.That(BuildCommands.HasFlag(Args, "-quit"), Is.False);
        }

        [Test]
        public void SplitList_TrimsAndSkipsEmpty()
        {
            Assert.That(BuildCommands.SplitList(" a, ,b ").ToArray(), Is.EqualTo(new[] { "a", "b" }));
        }
    }
}
