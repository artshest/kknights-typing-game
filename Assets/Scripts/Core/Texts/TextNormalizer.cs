using System;
using System.Text;

namespace KK.Core.Texts
{
    // При блокирующем вводе любой ненабираемый символ остановил бы игрока навсегда,
    // поэтому всё вне белого списка удаляется
    public static class TextNormalizer
    {
        public static string Normalize(string text, TextNormalizationOptions options)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            text = text.Normalize(NormalizationForm.FormC);
            
            var result = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                string replacement = Replace(ch, options);
                if (replacement == null)
                {
                    if (IsTypeable(ch))
                        AppendCollapsingSpaces(result, ch);
                    continue;
                }

                foreach (char replaced in replacement)
                    AppendCollapsingSpaces(result, replaced);
            }

            return result.ToString().Trim();
        }

        private static string Replace(char ch, TextNormalizationOptions options)
        {
            // Enter игрок не набирает, поэтому переводы строк тоже становятся пробелом
            if (char.IsWhiteSpace(ch))
                return " ";

            switch (ch)
            {
                case '«': case '»': case '„': case '“': case '”': case '‟': case '″':
                    return "\"";
                case '‘': case '’': case '‚': case '‛': case '′':
                    return "'";
                case '—': case '–': case '‒': case '―': case '−': case '‐': case '‑':
                    return "-";
                case '…':
                    return "...";
                case 'ё':
                    return options.ReplaceYo ? "е" : null;
                case 'Ё':
                    return options.ReplaceYo ? "Е" : null;
                default:
                    return null;
            }
        }

        private static bool IsTypeable(char ch)
        {
            return (ch >= ' ' && ch <= '~')
                || (ch >= 'А' && ch <= 'я')
                || ch == 'ё' || ch == 'Ё'
                || ch == '№';
        }

        private static void AppendCollapsingSpaces(StringBuilder builder, char ch)
        {
            if (ch == ' ' && (builder.Length == 0 || builder[builder.Length - 1] == ' '))
                return;

            builder.Append(ch);
        }
    }
}
