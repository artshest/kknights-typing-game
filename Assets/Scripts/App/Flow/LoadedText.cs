using KK.App.Texts;

namespace KK.App.Flow
{
    // Текст раунда вместе с источником: «Ещё раз» повторяет текст, «Другой текст» загружает новый из того же источника
    public readonly struct LoadedText
    {
        public readonly string Text;
        public readonly TextSource Source;

        public LoadedText(string text, TextSource source)
        {
            Text = text;
            Source = source;
        }
    }
}
