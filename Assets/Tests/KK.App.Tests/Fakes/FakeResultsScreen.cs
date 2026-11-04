using System;
using KK.App.Flow.Screens;
using KK.Core.Typing;

namespace KK.App.Tests.Fakes
{
    public class FakeResultsScreen : IResultsScreen
    {
        public event Action RestartRequested;
        public event Action NextTextRequested;

        public TypingResult? ShownResult { get; private set; }
        public bool HasSubscribers => RestartRequested != null || NextTextRequested != null;

        public void Show(TypingResult result)
        {
            ShownResult = result;
        }

        public void Hide()
        {
            ShownResult = null;
        }

        public void ClickRestart()
        {
            RestartRequested?.Invoke();
        }

        public void ClickNextText()
        {
            NextTextRequested?.Invoke();
        }
    }
}
