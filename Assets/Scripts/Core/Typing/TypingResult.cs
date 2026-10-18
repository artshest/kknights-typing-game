using System;

namespace KK.Core.Typing
{
    public readonly struct TypingResult
    {
        public readonly double CharactersPerMinute;
        // От 0 до 1, не проценты
        public readonly double Accuracy;
        public readonly int Mistakes;
        public readonly int CorrectCount;
        public readonly TimeSpan Duration;

        public TypingResult(double charactersPerMinute, double accuracy, int mistakes, int correctCount, TimeSpan duration)
        {
            CharactersPerMinute = charactersPerMinute;
            Accuracy = accuracy;
            Mistakes = mistakes;
            CorrectCount = correctCount;
            Duration = duration;
        }

        public override string ToString()
        {
            return $"{CharactersPerMinute:F0} зн/мин, точность {Accuracy:P1}, ошибок {Mistakes}, время {Duration.TotalSeconds:F1} с";
        }
    }
}
