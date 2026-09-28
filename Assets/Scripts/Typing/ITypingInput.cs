using System;

public interface ITypingInput
{
    public event Action<char> OnTextInput;
}