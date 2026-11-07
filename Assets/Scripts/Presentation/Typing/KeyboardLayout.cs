using System.Collections.Generic;

namespace KK.Presentation.Typing
{
    // Физическая клавиша русской раскладки ЙЦУКЕН для символа. Латиница и символы, которых нет в русской
    // раскладке, ищутся по той же физической клавише в QWERTY; при совпадении побеждает русская раскладка
    public static class KeyboardLayout
    {
        public const string Space = "space";
        public const string LeftShift = "lshift";
        public const string RightShift = "rshift";

        // Ряды: id клавиш (символ без Shift в русской раскладке), русские символы с Shift,
        // латинские без Shift и с Shift. Число клавиш левой руки — для выбора Shift противоположной руки
        private static readonly Row[] Rows =
        {
            new("ё1234567890-=", "Ё!\"№;%:?*()_+", "`1234567890-=", "~!@#$%^&*()_+", leftHandKeys: 6),
            new("йцукенгшщзхъ\\", "ЙЦУКЕНГШЩЗХЪ/", "qwertyuiop[]\\", "QWERTYUIOP{}|", leftHandKeys: 5),
            new("фывапролджэ", "ФЫВАПРОЛДЖЭ", "asdfghjkl;'", "ASDFGHJKL:\"", leftHandKeys: 5),
            new("ячсмитьбю.", "ЯЧСМИТЬБЮ,", "zxcvbnm,./", "ZXCVBNM<>?", leftHandKeys: 5),
        };

        private static readonly Dictionary<char, KeyPress> Keys = Build();

        public static bool TryFind(char ch, out string keyId, out string shiftKeyId)
        {
            if (Keys.TryGetValue(ch, out KeyPress press))
            {
                keyId = press.KeyId;
                shiftKeyId = press.ShiftKeyId;
                return true;
            }

            keyId = null;
            shiftKeyId = null;
            return false;
        }

        private static Dictionary<char, KeyPress> Build()
        {
            var keys = new Dictionary<char, KeyPress> { [' '] = new KeyPress(Space, null) };

            // Сначала вся русская раскладка, потом латинская: TryAdd не перезапишет русские символы
            foreach (Row row in Rows)
                AddLayout(keys, row, row.Ids, row.RuShifted);
            foreach (Row row in Rows)
                AddLayout(keys, row, row.EnLower, row.EnShifted);

            return keys;
        }

        private static void AddLayout(Dictionary<char, KeyPress> keys, Row row, string lower, string shifted)
        {
            for (int i = 0; i < row.Ids.Length; i++)
            {
                string keyId = row.Ids[i].ToString();
                // Shift жмут мизинцем другой руки, чтобы рука с символом не уходила с позиции
                string shiftKeyId = i < row.LeftHandKeys ? RightShift : LeftShift;

                keys.TryAdd(lower[i], new KeyPress(keyId, null));
                keys.TryAdd(shifted[i], new KeyPress(keyId, shiftKeyId));
            }
        }

        private readonly struct KeyPress
        {
            public readonly string KeyId;
            public readonly string ShiftKeyId;

            public KeyPress(string keyId, string shiftKeyId)
            {
                KeyId = keyId;
                ShiftKeyId = shiftKeyId;
            }
        }

        private readonly struct Row
        {
            public readonly string Ids;
            public readonly string RuShifted;
            public readonly string EnLower;
            public readonly string EnShifted;
            public readonly int LeftHandKeys;

            public Row(string ids, string ruShifted, string enLower, string enShifted, int leftHandKeys)
            {
                Ids = ids;
                RuShifted = ruShifted;
                EnLower = enLower;
                EnShifted = enShifted;
                LeftHandKeys = leftHandKeys;
            }
        }
    }
}
