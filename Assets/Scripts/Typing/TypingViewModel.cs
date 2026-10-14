using R3;
using System;
using System.Collections.Generic;
using System.Text;

public class TypingViewModel 
{
    public readonly string TypingText;
    public ReadOnlyReactiveProperty<int> CurrentIndex => _currentIndex;

    private readonly ReactiveProperty<int> _currentIndex = new(0);
    private ITypingInput _typingInput;
    private TypingSession _typingSession;

    public TypingViewModel(ITypingInput typingInput, TypingSession typingSession)
    {
        _typingInput = typingInput;
        _typingSession = typingSession;

        TypingText = _typingSession.TypingText;

        _typingInput.OnTextInput += _typingInput_OnTextInput;
    }

    private void _typingInput_OnTextInput(char obj)
    {
        if(_typingSession.ValidateTextInput(obj))
        {
            _currentIndex.Value = _typingSession.CurrentIndex;
        }
    }
}