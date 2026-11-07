using System;
using System.Collections.Generic;
using KK.App.Typing;

namespace KK.App.Tests.Fakes
{
    public class FakeTypingInput : ITypingInput
    {
        public event Action<char> OnTextInput;

        public bool HasSubscribers => OnTextInput != null;
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class FakeTypingInputFactory : ITypingInputFactory
    {
        public readonly List<FakeTypingInput> Created = new();

        public bool ReturnNull { get; set; }

        public FakeTypingInput Last => Created.Count > 0 ? Created[^1] : null;

        public ITypingInput Create()
        {
            if (ReturnNull)
                return null;

            var input = new FakeTypingInput();
            Created.Add(input);
            return input;
        }
    }
}
