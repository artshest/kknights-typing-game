using System;

namespace KK.Core.Typing
{
    public class TypingSession
    {
        public string TypingText { get => _typingText; }
        public int CurrentIndex { get => _currentIndex; }
        public bool IsCompleted => CurrentIndex >= TypingText.Length;

        private readonly string _typingText;
        private int _currentIndex;

        public TypingSession(string targetText)
        {
            if (string.IsNullOrEmpty(targetText))
                throw new ArgumentException("Текст для набора пустой", nameof(targetText));

            _typingText = targetText;
        }

        /// <summary>
        /// Пытается напечатать символ: при совпадении с ожидаемым сдвигает курсор и возвращает true.
        /// </summary>
        public bool TryType(char input)
        {
            if (IsCompleted) return false;

            bool correct = input == TypingText[CurrentIndex];
            if (correct)
            {
                _currentIndex++;
            }
            return correct;
        }
    }
}
