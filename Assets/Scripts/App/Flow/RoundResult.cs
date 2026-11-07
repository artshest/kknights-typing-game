using KK.Core.Typing;

namespace KK.App.Flow
{
    // Результатам нужен и текст с источником: из них решается, с чего начать следующий раунд
    public readonly struct RoundResult
    {
        public readonly TypingResult Result;
        public readonly LoadedText LoadedText;

        public RoundResult(TypingResult result, LoadedText loadedText)
        {
            Result = result;
            LoadedText = loadedText;
        }
    }
}
