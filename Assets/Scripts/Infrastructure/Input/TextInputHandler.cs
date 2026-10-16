using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextInputHandler : ITypingInput
{
    public event Action<char> OnTextInput;

    public TextInputHandler()
    {
        if (Keyboard.current == null)
        {
            Debug.LogError("There is no current Keyboard");
            return;
        }

        Keyboard.current.onTextInput += ValidateInput;
    }

    ~TextInputHandler()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= ValidateInput;
    }

    private void ValidateInput(char ch)
    {
        if (ch == '\b') // Backspace
        {
            return;
        }

        if (ch == '\r' || ch == '\n') // Enter
        {
            return;
        }

        OnTextInput?.Invoke(ch);
    }

}
