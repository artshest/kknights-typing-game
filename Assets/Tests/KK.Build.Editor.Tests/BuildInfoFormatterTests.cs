using System;
using NUnit.Framework;

namespace KK.Build.Editor.Tests
{
    public class BuildInfoFormatterTests
    {
        [TestCase(0, "0 Б")]
        [TestCase(1023, "1023 Б")]
        [TestCase(1536, "1,5 КБ")]
        [TestCase(12_897_484, "12,3 МБ")]
        [TestCase(3_221_225_472, "3,0 ГБ")]
        public void FormatSize_UsesBinaryUnitsAndComma(long bytes, string expected)
        {
            Assert.That(BuildInfoFormatter.FormatSize(bytes), Is.EqualTo(expected));
        }

        [Test]
        public void FormatSize_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BuildInfoFormatter.FormatSize(-1));
        }

        [Test]
        public void FullVersion_WithCommit_AppendsBuildMetadata()
        {
            Assert.That(BuildInfoFormatter.FullVersion(new BuildVersion(0, 1, 0), "3e48c23"), Is.EqualTo("0.1.0+3e48c23"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void FullVersion_WithoutCommit_IsPlainVersion(string commit)
        {
            Assert.That(BuildInfoFormatter.FullVersion(new BuildVersion(0, 1, 0), commit), Is.EqualTo("0.1.0"));
        }

        [Test]
        public void Format_ListsWebGLFilesAndPreBuildWarnings()
        {
            var info = new BuildInfo
            {
                ProductName = "KeyboardKnights",
                Version = new BuildVersion(0, 1, 0),
                Commit = "abc1234",
                ProfileName = "WebGL-Release",
                Platform = "WebGL",
                Development = false,
                Result = "Succeeded",
                ShippedSize = 2048,
            };
            info.Files.Add(("Build/game.wasm.br", 1024));
            info.PreBuildWarnings.Add("Есть незакоммиченные изменения");

            var text = BuildInfoFormatter.Format(info);

            Assert.That(text, Does.Contain("KeyboardKnights 0.1.0+abc1234"));
            Assert.That(text, Does.Contain("Размер для игрока: 2,0 КБ"));
            Assert.That(text, Does.Contain("Build/game.wasm.br — 1,0 КБ"));
            Assert.That(text, Does.Contain("- Есть незакоммиченные изменения"));
        }
    }
}
