namespace KK.Presentation.Typing
{
    // Плашка всегда шириной в ячейку символа (продвижение курсора), но стоит по центру того, что видно:
    // у пиксельных шрифтов промежуток между буквами только справа, и ячейка от origin сдвинута вправо
    public static class CaretLayout
    {
        public static (float left, float right) Glyph(float origin, float xAdvance, float glyphLeft, float glyphRight)
        {
            return Centered((glyphLeft + glyphRight) / 2f, xAdvance - origin);
        }

        // У пробела глифа нет — центр между соседними буквами строки. Соседа нет (начало или конец строки) —
        // эта сторона берётся по ячейке
        public static (float left, float right) Space(float origin, float xAdvance, float? previousGlyphRight, float? nextGlyphLeft)
        {
            float from = previousGlyphRight ?? origin;
            float to = nextGlyphLeft ?? xAdvance;
            return Centered((from + to) / 2f, xAdvance - origin);
        }

        private static (float left, float right) Centered(float center, float width)
        {
            return (center - width / 2f, center + width / 2f);
        }
    }
}
