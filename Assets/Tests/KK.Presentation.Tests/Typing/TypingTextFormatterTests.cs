using System;
using KK.Presentation.Typing;
using NUnit.Framework;

namespace KK.Presentation.Tests.Typing
{
    public class TypingTextFormatterTests
    {
        [Test]
        public void Format_AtStart_HighlightsFirstCharWithoutTypedPart()
        {
            string result = TypingTextFormatter.Format("abc", 0, false);

            Assert.That(result, Is.EqualTo("<color=#8E8B9B><color=#1A1208>a</color>bc</color>"));
        }

        [Test]
        public void Format_InMiddle_SplitsIntoThreeParts()
        {
            string result = TypingTextFormatter.Format("abc", 1, false);

            Assert.That(result, Is.EqualTo("<color=#F2F0F7>a</color><color=#8E8B9B><color=#1A1208>b</color>c</color>"));
        }

        [Test]
        public void Format_Mistake_UsesMistakeColorForCurrentChar()
        {
            string result = TypingTextFormatter.Format("abc", 1, true);

            Assert.That(result, Is.EqualTo(
                "<color=#F2F0F7>a</color><color=#8E8B9B><color=#F2F0F7>b</color>c</color>"));
        }

        [Test]
        public void Format_Completed_ShowsOnlyTypedPart()
        {
            string result = TypingTextFormatter.Format("ab", 2, false);

            Assert.That(result, Is.EqualTo("<color=#F2F0F7>ab</color>"));
        }

        [Test]
        public void Format_TagStart_IsEscaped()
        {
            string result = TypingTextFormatter.Format("<b>", 0, false);

            Assert.That(result, Is.EqualTo("<color=#8E8B9B><color=#1A1208><noparse><</noparse></color>b></color>"));
        }

        [Test]
        public void Format_Backslash_BreaksEscapeSequence()
        {
            string result = TypingTextFormatter.Format(@"\u", 0, false);

            Assert.That(result, Is.EqualTo(@"<color=#8E8B9B><color=#1A1208>\<noparse></noparse></color>u</color>"));
        }

        [Test]
        public void Format_NullText_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TypingTextFormatter.Format(null, 0, false));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Format_IndexOutOfRange_Throws(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TypingTextFormatter.Format("abc", index, false));
        }
    }
}
