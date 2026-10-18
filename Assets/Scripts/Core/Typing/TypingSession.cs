using System;
using System.Collections.Generic;

namespace KK.Core.Typing
{
    public class TypingSession
    {
        public string TypingText { get => _typingText; }
        public int CurrentIndex { get => _currentIndex; }
        public bool IsCompleted => CurrentIndex >= TypingText.Length;
        public IReadOnlyList<Keystroke> Keystrokes => _keystrokes;

        private readonly string _typingText;
        private readonly List<Keystroke> _keystrokes = new();
        private int _currentIndex;

        public TypingSession(string targetText)
        {
            if (string.IsNullOrEmpty(targetText))
                throw new ArgumentException("Текст для набора пустой", nameof(targetText));

            _typingText = targetText;
        }

        public bool TryType(char input, TimeSpan time, out Keystroke keystroke)
        {
            if (IsCompleted)
            {
                keystroke = default;
                return false;
            }

            keystroke = new Keystroke(_currentIndex, _typingText[_currentIndex], input, time);
            _keystrokes.Add(keystroke);

            if (keystroke.IsCorrect)
            {
                _currentIndex++;
            }
            return true;
        }

        public TypingResult GetResult(TimeSpan now)
        {
            // После завершения now игнорируется, иначе итоговая скорость падала бы с течением времени
            TimeSpan end = IsCompleted ? _keystrokes[_keystrokes.Count - 1].Time : now;
            return StatsCalculator.Calculate(_keystrokes, end);
        }
    }
}
