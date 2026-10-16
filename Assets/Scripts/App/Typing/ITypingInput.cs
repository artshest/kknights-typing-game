using System;

namespace KK.App.Typing
{
    public interface ITypingInput
    {
        //public ReactiveProperty<char> TextInput;

        public event Action<char> OnTextInput;
    }
}
