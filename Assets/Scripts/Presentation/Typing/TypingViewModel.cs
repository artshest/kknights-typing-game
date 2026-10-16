using KK.App.Typing;
using KK.Core.Typing;
using R3;
using System;

namespace KK.Presentation.Typing
{
    public class TypingViewModel : IDisposable
    {
        public readonly string TypingText;
        public ReadOnlyReactiveProperty<int> CurrentIndex => _currentIndex;

        private readonly ReactiveProperty<int> _currentIndex = new(0);
        private readonly ITypingInput _typingInput;
        private readonly TypingSession _typingSession;

        public TypingViewModel(ITypingInput typingInput, TypingSession typingSession)
        {
            _typingInput = typingInput;
            _typingSession = typingSession;

            TypingText = _typingSession.TypingText;

            _typingInput.OnTextInput += HandleTextInput;
        }

        private void HandleTextInput(char input)
        {
            if (_typingSession.TryType(input))
            {
                _currentIndex.Value = _typingSession.CurrentIndex;
            }
        }

        public void Dispose()
        {
            _typingInput.OnTextInput -= HandleTextInput;
            _currentIndex.Dispose();
        }
    }
}
