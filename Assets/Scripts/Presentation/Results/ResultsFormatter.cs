using System;
using System.Globalization;

namespace KK.Presentation.Results
{
    public static class ResultsFormatter
    {
        // Разделитель задан явно: CultureInfo.CurrentCulture зависит от системы игрока,
        // а культуры "ru-RU" в WebGL-сборке может не оказаться
        private static readonly NumberFormatInfo NumberFormat = new() { NumberDecimalSeparator = "," };

        public static string FormatSpeed(double charactersPerMinute)
        {
            return $"{charactersPerMinute.ToString("F0", NumberFormat)} зн/мин";
        }

        public static string FormatAccuracy(double accuracy)
        {
            return $"{(accuracy * 100).ToString("F1", NumberFormat)} %";
        }

        public static string FormatDuration(TimeSpan duration)
        {
            int totalSeconds = (int)Math.Round(duration.TotalSeconds);
            return $"{totalSeconds / 60}:{totalSeconds % 60:00}";
        }

        public static string FormatMistakes(int mistakes)
        {
            return mistakes.ToString(CultureInfo.InvariantCulture);
        }
    }
}
