using R3;
using System;
using System.Collections.Generic;
using System.Text;

public class TypingViewModel
{
    public ReactiveProperty<string> TargetText = new ReactiveProperty<string>();
    public ReactiveProperty<string> RemainingText = new ReactiveProperty<string>();
    public ReactiveProperty<char> LastInput = new ReactiveProperty<char>();
    public ReactiveProperty<char> LastCorrectInput = new ReactiveProperty<char>();
    public ReactiveProperty<char> LastIncorrectInput = new ReactiveProperty<char>();

    private ITypingInput _typingInput;
    private TypingSession _typingSession;

    public TypingViewModel(ITypingInput typingInput, TypingSession typingSession)
    {
        _typingInput = typingInput;
        _typingSession = typingSession;

        _typingInput.OnTextInput += _typingInput_OnTextInput;

        TargetText.Value = _typingSession.TargetText;
        RemainingText.Value = _typingSession.RemainingText;
    }

    private void _typingInput_OnTextInput(char obj)
    {
        _typingSession.ValidateTextInput(obj, out string remainingText, out bool isComplete);

        RemainingText.Value = remainingText;
    }
}