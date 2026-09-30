using R3;
using System;

public interface ITypingInput
{
    //public ReactiveProperty<char> TextInput;

    public event Action<char> OnTextInput;
}