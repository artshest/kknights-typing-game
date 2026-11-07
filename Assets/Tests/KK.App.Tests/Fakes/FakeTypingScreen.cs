using System;
using KK.App.Flow.Screens;
using KK.App.Typing;
using KK.Core.Typing;

namespace KK.App.Tests.Fakes
{
    public class FakeTypingScreen : ITypingScreen
    {
        public event Action<TypingResult> Completed;

        public TypingSession Session { get; private set; }
        public ITypingInput Input { get; private set; }
        // Был ли ввод уже освобождён в момент End: ViewModel должна отписаться от живого ввода
        public bool? InputDisposedAtEnd { get; private set; }
        public int BeginCount { get; private set; }
        public bool HasCompletedSubscribers => Completed != null;

        public void Begin(TypingSession session, ITypingInput input)
        {
            Session = session;
            Input = input;
            BeginCount++;
        }

        public void End()
        {
            if (Input is FakeTypingInput fake)
                InputDisposedAtEnd = fake.IsDisposed;
            Session = null;
            Input = null;
        }

        public void Complete(TypingResult result)
        {
            Completed?.Invoke(result);
        }
    }
}
