using System;
using System.Text;

namespace KK.Presentation.Typing
{
    public static class TypingTextFormatter
    {
        private const string TypedOpen = "<color=#FFFFFF33>";
        private const string RemainingOpen = "<color=#FF9E4A>";
        private const string ColorClose = "</color>";

        private const string CurrentOpen = "<u>";
        private const string CurrentClose = "</u>";

        // Фон, а не только цвет: иначе ошибка на пробеле не видна
        private const string MistakeOpen = "<color=#FF4D4D><mark=#FF4D4D40>";
        private const string MistakeClose = "</mark></color>";

        // Тег TMP открывается только символом '<', поэтому достаточно защитить каждый '<' —
        // это закрывает и «</noparse>» внутри самого текста. &lt; TMP не поддерживает
        private const string EscapedTagStart = "<noparse><</noparse>";

        // TMP разбирает \uXXXX даже при parseCtrlCharacters = false. Пустой тег после слэша разрывает
        // последовательность (за слэшем уже не буква), а при разборе тегов исчезает сам
        private const string EscapeSequenceBreak = "<noparse></noparse>";

        public static string Format(string text, int currentIndex, bool hasMistake)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (currentIndex < 0 || currentIndex > text.Length)
                throw new ArgumentOutOfRangeException(nameof(currentIndex));

            var builder = new StringBuilder(text.Length * 2);

            if (currentIndex > 0)
            {
                builder.Append(TypedOpen);
                AppendEscaped(builder, text, 0, currentIndex);
                builder.Append(ColorClose);
            }

            if (currentIndex < text.Length)
            {
                builder.Append(RemainingOpen);
                builder.Append(hasMistake ? MistakeOpen : CurrentOpen);
                AppendEscaped(builder, text, currentIndex, 1);
                builder.Append(hasMistake ? MistakeClose : CurrentClose);
                AppendEscaped(builder, text, currentIndex + 1, text.Length - currentIndex - 1);
                builder.Append(ColorClose);
            }

            return builder.ToString();
        }

        private static void AppendEscaped(StringBuilder builder, string text, int start, int length)
        {
            for (int i = start; i < start + length; i++)
            {
                if (text[i] == '<')
                {
                    builder.Append(EscapedTagStart);
                }
                else
                {
                    builder.Append(text[i]);
                    if (text[i] == '\\')
                        builder.Append(EscapeSequenceBreak);
                }
            }
        }
    }
}
