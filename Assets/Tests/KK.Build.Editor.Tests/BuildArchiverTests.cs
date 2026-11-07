using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;

namespace KK.Build.Editor.Tests
{
    public class BuildArchiverTests
    {
        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "KK.Build.Tests." + Path.GetRandomFileName());
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        [TestCase("KeyboardKnights_BurstDebugInformation_DoNotShip/lib.pdb")]
        [TestCase("KeyboardKnights_BackUpThisFolder_ButDontShipItWithYourGame\\cpp\\a.cpp")]
        public void ShouldExclude_DoNotShipFolders_ReturnsTrue(string path)
        {
            Assert.That(BuildArchiver.ShouldExclude(path), Is.True);
        }

        [TestCase("index.html")]
        [TestCase("Build/Game.wasm.br")]
        [TestCase("KeyboardKnights_Data/Managed/Assembly-CSharp.dll")]
        [TestCase("notes_BurstDebugInformation_DoNotShip")]
        public void ShouldExclude_RegularFiles_ReturnsFalse(string path)
        {
            Assert.That(BuildArchiver.ShouldExclude(path), Is.False);
        }

        [Test]
        public void CreateZip_PutsFilesAtRootWithForwardSlashesAndSkipsDoNotShip()
        {
            var game = Path.Combine(_root, "Game");
            Directory.CreateDirectory(Path.Combine(game, "Build"));
            Directory.CreateDirectory(Path.Combine(game, "X_BurstDebugInformation_DoNotShip"));
            File.WriteAllText(Path.Combine(game, "index.html"), "<html/>");
            File.WriteAllText(Path.Combine(game, "Build", "game.wasm"), "wasm");
            File.WriteAllText(Path.Combine(game, "X_BurstDebugInformation_DoNotShip", "lib.pdb"), "pdb");
            var zipPath = Path.Combine(_root, "game.zip");

            BuildArchiver.CreateZip(game, zipPath);

            using var archive = ZipFile.OpenRead(zipPath);
            var names = archive.Entries.Select(e => e.FullName).OrderBy(n => n).ToArray();
            Assert.That(names, Is.EqualTo(new[] { "Build/game.wasm", "index.html" }));
        }

        [Test]
        public void GetShippedSize_SkipsDoNotShipFolders()
        {
            var game = Path.Combine(_root, "Game");
            Directory.CreateDirectory(Path.Combine(game, "X_BackUpThisFolder_ButDontShipItWithYourGame"));
            File.WriteAllText(Path.Combine(game, "index.html"), "12345");
            File.WriteAllText(Path.Combine(game, "X_BackUpThisFolder_ButDontShipItWithYourGame", "a.cpp"), "1234567890");

            Assert.That(BuildArchiver.GetShippedSize(game), Is.EqualTo(5));
        }

        [Test]
        public void CreateZip_ExistingArchive_IsReplaced()
        {
            var game = Path.Combine(_root, "Game");
            Directory.CreateDirectory(game);
            File.WriteAllText(Path.Combine(game, "index.html"), "<html/>");
            var zipPath = Path.Combine(_root, "game.zip");
            File.WriteAllText(zipPath, "old");

            BuildArchiver.CreateZip(game, zipPath);

            using var archive = ZipFile.OpenRead(zipPath);
            Assert.That(archive.Entries.Count, Is.EqualTo(1));
        }
    }
}
