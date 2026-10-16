using System;

namespace KK.App.Typing
{
    public interface ITypingInput : IDisposable
    {
        public event Action<char> OnTextInput;
    }
}
