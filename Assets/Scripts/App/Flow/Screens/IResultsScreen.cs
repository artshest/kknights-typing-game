using KK.Core.Typing;
using System;

namespace KK.App.Flow.Screens
{
    public interface IResultsScreen
    {
        event Action RestartRequested;
        event Action NextTextRequested;
        event Action MenuRequested;

        void Show(TypingResult result);
        void Hide();
    }
}
