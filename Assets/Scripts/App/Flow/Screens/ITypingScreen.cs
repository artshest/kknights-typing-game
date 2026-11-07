using KK.App.Typing;
using KK.Core.Typing;
using System;

namespace KK.App.Flow.Screens
{
    // TODO: раунд ведёт экран, потому что логика ввода пока живёт во ViewModel (claude/TODO/typing-round-to-app-layer.md)
    public interface ITypingScreen
    {
        event Action<TypingResult> Completed;

        // Ввод создаёт и освобождает TypingState, экран только слушает его до End
        void Begin(TypingSession session, ITypingInput input);
        // Не прячет набранный текст: он остаётся фоном под результатами и загрузкой
        void End();
    }
}
