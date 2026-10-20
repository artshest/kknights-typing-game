using System;
using KK.Core.Typing;
using KK.Presentation.Tests.Fakes;
using KK.Presentation.Typing;
using NUnit.Framework;

namespace KK.Presentation.Tests.Typing
{
    public class TypingViewModelTests
    {
        private FakeTypingInput _input;
        private FakeClock _clock;
        private TypingViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _input = new FakeTypingInput();
            _clock = new FakeClock();
            _viewModel = new TypingViewModel(_input, new TypingSession("ab"), _clock);
        }

        [TearDown]
        public void TearDown()
        {
            _viewModel.Dispose();
        }

        [Test]
        public void TextInput_CorrectChar_MovesCurrentIndex()
        {
            _input.Type('a');

            Assert.That(_viewModel.CurrentIndex.CurrentValue, Is.EqualTo(1));
        }

        [Test]
        public void TextInput_CorrectCharAfterWrong_ResetsHasMistake()
        {
            _input.Type('x');
            _input.Type('a');

            Assert.That(_viewModel.HasMistake.CurrentValue, Is.False);
        }

        [Test]
        public void TextInput_WrongChar_KeepsCurrentIndex()
        {
            _input.Type('x');

            Assert.That(_viewModel.CurrentIndex.CurrentValue, Is.EqualTo(0));
        }

        [Test]
        public void TextInput_WrongChar_SetsHasMistake()
        {
            _input.Type('x');

            Assert.That(_viewModel.HasMistake.CurrentValue, Is.True);
        }

        [Test]
        public void TextInput_LastChar_RaisesCompletedWithResult()
        {
            TypingResult? result = null;
            _viewModel.Completed += r => result = r;

            _input.Type('a');
            _clock.Advance(2);
            _input.Type('b');

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.CorrectCount, Is.EqualTo(2));
            Assert.That(result.Value.Duration, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void Dispose_UnsubscribesFromInput()
        {
            _viewModel.Dispose();

            Assert.That(_input.HasSubscribers, Is.False);
        }
    }
}
