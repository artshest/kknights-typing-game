using KK.Core.Typing;

namespace KK.App.Flow
{
    // Результатам нужен и текст: «Ещё раз» начинает новый раунд с ним же
    public readonly struct RoundResult
    {
        public readonly TypingResult Result;
        public readonly string Text;

        public RoundResult(TypingResult result, string text)
        {
            Result = result;
            Text = text;
        }
    }
}
