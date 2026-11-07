using KK.Presentation.Typing;
using NUnit.Framework;

namespace KK.Presentation.Tests.Typing
{
    public class KeyboardLayoutTests
    {
        [TestCase('а', "а")]
        [TestCase('ж', "ж")]
        [TestCase('ё', "ё")]
        [TestCase('5', "5")]
        [TestCase('.', ".")]
        [TestCase('-', "-")]
        public void TryFind_UnshiftedChar_ReturnsKeyWithoutShift(char ch, string expectedKey)
        {
            bool found = KeyboardLayout.TryFind(ch, out string keyId, out string shiftKeyId);

            Assert.That(found, Is.True);
            Assert.That(keyId, Is.EqualTo(expectedKey));
            Assert.That(shiftKeyId, Is.Null);
        }

        [Test]
        public void TryFind_Space_ReturnsSpaceKey()
        {
            KeyboardLayout.TryFind(' ', out string keyId, out string shiftKeyId);

            Assert.That(keyId, Is.EqualTo(KeyboardLayout.Space));
            Assert.That(shiftKeyId, Is.Null);
        }

        // Shift — противоположной руки: «Ф» слева, значит правый Shift
        [TestCase('Ф', "ф", KeyboardLayout.RightShift)]
        [TestCase('Ё', "ё", KeyboardLayout.RightShift)]
        [TestCase('%', "5", KeyboardLayout.RightShift)]
        [TestCase('Ж', "ж", KeyboardLayout.LeftShift)]
        [TestCase(',', ".", KeyboardLayout.LeftShift)]
        [TestCase('№', "3", KeyboardLayout.RightShift)]
        [TestCase('?', "7", KeyboardLayout.LeftShift)]
        public void TryFind_ShiftedChar_ReturnsOppositeHandShift(char ch, string expectedKey, string expectedShift)
        {
            KeyboardLayout.TryFind(ch, out string keyId, out string shiftKeyId);

            Assert.That(keyId, Is.EqualTo(expectedKey));
            Assert.That(shiftKeyId, Is.EqualTo(expectedShift));
        }

        // Латиница — та же физическая клавиша, что и в QWERTY
        [TestCase('q', "й", null)]
        [TestCase('f', "а", null)]
        [TestCase('M', "ь", KeyboardLayout.LeftShift)]
        [TestCase('@', "2", KeyboardLayout.RightShift)]
        [TestCase('[', "х", null)]
        public void TryFind_LatinChar_ReturnsSamePhysicalKey(char ch, string expectedKey, string expectedShift)
        {
            KeyboardLayout.TryFind(ch, out string keyId, out string shiftKeyId);

            Assert.That(keyId, Is.EqualTo(expectedKey));
            Assert.That(shiftKeyId, Is.EqualTo(expectedShift));
        }

        // Символ есть в обеих раскладках на разных клавишах — подсвечивается русская
        [TestCase(';', "4")]
        [TestCase('"', "2")]
        [TestCase('/', "\\")]
        public void TryFind_CharInBothLayouts_PrefersRussian(char ch, string expectedKey)
        {
            KeyboardLayout.TryFind(ch, out string keyId, out _);

            Assert.That(keyId, Is.EqualTo(expectedKey));
        }

        [Test]
        public void TryFind_UnknownChar_ReturnsFalse()
        {
            bool found = KeyboardLayout.TryFind('€', out string keyId, out string shiftKeyId);

            Assert.That(found, Is.False);
            Assert.That(keyId, Is.Null);
            Assert.That(shiftKeyId, Is.Null);
        }
    }
}
