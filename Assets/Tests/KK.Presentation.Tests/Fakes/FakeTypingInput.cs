using System;
using KK.App.Typing;

namespace KK.Presentation.Tests.Fakes
{
    public class FakeTypingInput : ITypingInput
    {
        public event Action<char> OnTextInput;

        public bool HasSubscribers => OnTextInput != null;
        public bool IsDisposed { get; private set; }

        public void Type(char ch)
        {
            OnTextInput?.Invoke(ch);
        }

        public void Type(string text)
        {
            foreach (char ch in text)
                Type(ch);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
