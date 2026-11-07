using System;
using System.Globalization;

namespace KK.Presentation.Typing
{
    // Живая статистика в шапке экрана набора: короткие числа без единиц — подписи стоят отдельно
    public static class TypingStatsFormatter
    {
        public static string FormatSpeed(double charactersPerMinute)
        {
            return Math.Round(charactersPerMinute).ToString(CultureInfo.InvariantCulture);
        }

        // accuracy — от 0 до 1
        public static string FormatAccuracy(double accuracy)
        {
            return $"{Math.Round(accuracy * 100).ToString(CultureInfo.InvariantCulture)}%";
        }

        public static string FormatMistakes(int mistakes)
        {
            return mistakes.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatSeconds(TimeSpan duration)
        {
            return ((int)duration.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatTypedCount(int typed, int total)
        {
            return $"Символы: {typed.ToString(CultureInfo.InvariantCulture)} / {total.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
