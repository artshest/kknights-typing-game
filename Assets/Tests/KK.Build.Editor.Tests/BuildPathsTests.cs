using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace KK.Build.Editor.Tests
{
    public class BuildPathsTests
    {
        private static readonly BuildVersion Version = new BuildVersion(0, 1, 0);
        private static readonly DateTime Time = new DateTime(2026, 11, 8, 9, 5, 42);

        [Test]
        public void GetBuildName_ContainsProductVersionAndMinuteStamp()
        {
            Assert.That(BuildPaths.GetBuildName("KeyboardKnights", Version, Time),
                Is.EqualTo("KeyboardKnights_0.1.0_20261108-0905"));
        }

        [Test]
        public void GetBuildName_ReplacesSpacesInProductName()
        {
            Assert.That(BuildPaths.GetBuildName("Keyboard Knights", Version, Time),
                Is.EqualTo("Keyboard_Knights_0.1.0_20261108-0905"));
        }

        [Test]
        public void GetBuildDirectory_GroupsByProfile()
        {
            var directory = BuildPaths.GetBuildDirectory("root", "WebGL-Release", "Game_0.1.0_20261108-0905");

            Assert.That(directory, Is.EqualTo(Path.Combine("root", "Builds", "WebGL-Release", "Game_0.1.0_20261108-0905")));
        }

        [Test]
        public void GetPlayerLocation_Windows_PointsToExeInsideGameFolder()
        {
            var location = BuildPaths.GetPlayerLocation("build", BuildTarget.StandaloneWindows64, "KeyboardKnights");

            Assert.That(location, Is.EqualTo(Path.Combine("build", "Game", "KeyboardKnights.exe")));
        }

        [Test]
        public void GetPlayerLocation_WebGL_IsGameFolder()
        {
            var location = BuildPaths.GetPlayerLocation("build", BuildTarget.WebGL, "KeyboardKnights");

            Assert.That(location, Is.EqualTo(Path.Combine("build", "Game")));
        }

        [Test]
        public void GetArchivePath_IsNextToGameFolder()
        {
            Assert.That(BuildPaths.GetArchivePath("build", "Game_0.1.0"), Is.EqualTo(Path.Combine("build", "Game_0.1.0.zip")));
        }

        [TestCase("a:b", "a_b")]
        [TestCase("a/b", "a_b")]
        [TestCase("  name  ", "name")]
        [TestCase("a\tb", "a_b")]
        public void SanitizeFileName_ReplacesInvalidCharacters(string name, string expected)
        {
            Assert.That(BuildPaths.SanitizeFileName(name), Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void SanitizeFileName_Empty_Throws(string name)
        {
            Assert.Throws<ArgumentException>(() => BuildPaths.SanitizeFileName(name));
        }
    }
}
