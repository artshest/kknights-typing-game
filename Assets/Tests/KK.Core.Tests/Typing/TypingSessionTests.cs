using System;
using KK.Core.Typing;
using NUnit.Framework;

namespace KK.Core.Tests.Typing
{
    public class TypingSessionTests
    {
        private TypingSession _session;

        [SetUp]
        public void SetUp()
        {
            _session = new TypingSession("ab");
        }

        [Test]
        public void Constructor_EmptyText_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TypingSession(""));
        }

        [Test]
        public void Constructor_Null_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TypingSession(null));
        }

        [Test]
        public void TryType_CorrectChar_MovesCursor()
        {
            _session.TryType('a', TimeSpan.FromSeconds(1), out _);

            Assert.That(_session.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void TryType_WrongChar_AddsKeystroke()
        {
            _session.TryType('c', TimeSpan.FromSeconds(1), out _);

            Keystroke keystroke = _session.Keystrokes[0];

            Assert.That(keystroke.Expected, Is.EqualTo('a'));
            Assert.That(keystroke.Typed, Is.EqualTo('c'));
        }

        [Test]
        public void TryType_LastChar_CompletesSession()
        {
            _session.TryType('a', TimeSpan.FromSeconds(1), out _);
            _session.TryType('b', TimeSpan.FromSeconds(1), out _);

            Assert.That(_session.IsCompleted, Is.True);
        }

        [Test]
        public void TryType_SessionCompleted_ReturnsFalse()
        {
            _session.TryType('a', TimeSpan.FromSeconds(1), out _);
            _session.TryType('b', TimeSpan.FromSeconds(1), out _);

            bool typed = _session.TryType('a', TimeSpan.FromSeconds(1), out _);

            Assert.That(typed, Is.False);
            Assert.That(_session.Keystrokes.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryType_WrongChar_RecordsMistakeWithoutMovingCursor()
        {
            _session.TryType('x', TimeSpan.FromSeconds(1), out Keystroke keystroke);

            Assert.That(_session.CurrentIndex, Is.EqualTo(0));
            Assert.That(keystroke.IsCorrect, Is.False);
            Assert.That(_session.Keystrokes.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetResult_SessionCompleted_IgnoresNow()
        {
            _session.TryType('a', TimeSpan.FromSeconds(0), out _);
            _session.TryType('b', TimeSpan.FromSeconds(1), out _);

            TypingResult result = _session.GetResult(TimeSpan.FromSeconds(100));

            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void GetResult_SessionNotCompleted_UsesNow()
        {
            _session.TryType('a', TimeSpan.FromSeconds(1), out _);

            TypingResult result = _session.GetResult(TimeSpan.FromSeconds(3));

            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }
    }
}