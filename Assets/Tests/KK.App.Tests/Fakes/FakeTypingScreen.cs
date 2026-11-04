using System;
using KK.App.Flow.Screens;
using KK.Core.Typing;

namespace KK.App.Tests.Fakes
{
    public class FakeTypingScreen : ITypingScreen
    {
        public event Action<TypingResult> Completed;

        public TypingSession Session { get; private set; }
        public int BeginCount { get; private set; }
        public bool HasCompletedSubscribers => Completed != null;

        public void Begin(TypingSession session)
        {
            Session = session;
            BeginCount++;
        }

        public void End()
        {
            Session = null;
        }

        public void Complete(TypingResult result)
        {
            Completed?.Invoke(result);
        }
    }
}
