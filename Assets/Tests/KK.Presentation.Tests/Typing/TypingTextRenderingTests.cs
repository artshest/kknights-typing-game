using System.Text;
using KK.Presentation.Typing;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace KK.Presentation.Tests.Typing
{
    // Строковые тесты форматтера не доказывают, что TMP покажет именно эти символы, надо проверять на реальном компоненте
    public class TypingTextRenderingTests
    {
        private GameObject _gameObject;
        private TextMeshPro _textMesh;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TypingTextRenderingTests");
            _textMesh = _gameObject.AddComponent<TextMeshPro>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [TestCase("a<b>c</b>d")]
        [TestCase("x</noparse>y<color=red>z")]
        [TestCase("<<u>>")]
        [TestCase("<noparse>")]
        [TestCase(@"path\nname\t\u0041\U0001F600\\n")]
        public void Format_TextWithMarkup_RendersSourceCharsAsIs(string text)
        {
            for (int index = 0; index <= text.Length; index++)
            {
                AssertRendersAsIs(text, index, hasMistake: false);
                AssertRendersAsIs(text, index, hasMistake: true);
            }
        }

        private void AssertRendersAsIs(string text, int index, bool hasMistake)
        {
            _textMesh.text = TypingTextFormatter.Format(text, index, hasMistake);
            _textMesh.ForceMeshUpdate();

            Assert.That(RenderedChars(), Is.EqualTo(text), $"index={index}, hasMistake={hasMistake}");
        }

        private string RenderedChars()
        {
            TMP_TextInfo info = _textMesh.textInfo;
            var builder = new StringBuilder(info.characterCount);
            for (int i = 0; i < info.characterCount; i++)
                builder.Append(info.characterInfo[i].character);
            return builder.ToString();
        }
    }
}
