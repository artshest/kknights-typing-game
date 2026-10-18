using System;
using System.Collections.Generic;

namespace KK.Core.Typing
{
    public static class StatsCalculator
    {
        public static TypingResult Calculate(IReadOnlyList<Keystroke> keystrokes, TimeSpan endTime)
        {
            if (keystrokes == null)
                throw new ArgumentNullException(nameof(keystrokes));

            if (keystrokes.Count == 0)
                return new TypingResult(0, 1, 0, 0, TimeSpan.Zero);

            int correct = 0;
            foreach (Keystroke keystroke in keystrokes)
            {
                if (keystroke.IsCorrect)
                    correct++;
            }

            int mistakes = keystrokes.Count - correct;
            double accuracy = (double)correct / keystrokes.Count;

            // Отсчёт от первого нажатия, а не от показа текста: чтение перед стартом не снижает скорость
            TimeSpan duration = endTime - keystrokes[0].Time;
            if (duration < TimeSpan.Zero)
                duration = TimeSpan.Zero;

            double cpm = duration > TimeSpan.Zero ? correct / duration.TotalMinutes : 0;

            return new TypingResult(cpm, accuracy, mistakes, correct, duration);
        }
    }
}
