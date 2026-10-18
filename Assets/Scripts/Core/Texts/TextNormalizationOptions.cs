namespace KK.Core.Texts
{
    public class TextNormalizationOptions
    {
        // TODO: брать из настроек игрока
        public bool ReplaceYo = true;

        public static TextNormalizationOptions Default => new();
    }
}
