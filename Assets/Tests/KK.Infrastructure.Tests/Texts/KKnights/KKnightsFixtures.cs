using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KK.Infrastructure.Tests.Texts.KKnights
{
    public static class KKnightsFixtures
    {
        public const string Blocks = "1508-staroe-dobroe-fentezi.json";
        public const string Document = "post5235-valor-mortis.json";

        private const string FixturesPath = "Assets/Tests/KK.Infrastructure.Tests/Texts/KKnights/Fixtures/";

        public static string Load(string fileName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(FixturesPath + fileName);
            Assert.That(asset, Is.Not.Null, $"Фикстура {fileName} не найдена");
            return asset.text;
        }
    }
}
