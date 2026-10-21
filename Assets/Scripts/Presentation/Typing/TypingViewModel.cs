using KK.App.Timing;
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
        public ReadOnlyReactiveProperty<bool> HasMistake => _hasMistake;
        public ReadOnlyReactiveProperty<TypingResult> Stats => _stats;

        public event Action<TypingResult> Completed;

        private readonly ReactiveProperty<int> _currentIndex = new(0);
        private readonly ReactiveProperty<bool> _hasMistake = new(false);
        private readonly ReactiveProperty<TypingResult> _stats;
        private readonly ITypingInput _typingInput;
        private readonly TypingSession _typingSession;
        private readonly IClock _clock;

        public TypingViewModel(ITypingInput typingInput, TypingSession typingSession, IClock clock)
        {
            _typingInput = typingInput;
            _typingSession = typingSession;
            _clock = clock;

            TypingText = _typingSession.TypingText;
            _stats = new ReactiveProperty<TypingResult>(_typingSession.GetResult(_clock.Now));

            _typingInput.OnTextInput += HandleTextInput;
        }

        private void HandleTextInput(char input)
        {
            if (!_typingSession.TryType(input, _clock.Now, out Keystroke keystroke))
                return;

            _stats.Value = _typingSession.GetResult(_clock.Now);

            if (!keystroke.IsCorrect)
            {
                _hasMistake.Value = true;
                return;
            }

            _hasMistake.Value = false;
            _currentIndex.Value = _typingSession.CurrentIndex;

            if (_typingSession.IsCompleted)
                Completed?.Invoke(_stats.Value);
        }

        public void Tick()
        {
            _stats.Value = _typingSession.GetResult(_clock.Now);
        }

        public void Dispose()
        {
            _typingInput.OnTextInput -= HandleTextInput;
            _currentIndex.Dispose();
            _hasMistake.Dispose();
            _stats.Dispose();
        }
    }
}
