using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TypingSession
{
    public string TypingText { get => _typingText; }
    public int CurrentIndex { get => _currentIndex; }
    public bool IsCompleted => CurrentIndex >= TypingText.Length;

    private string _typingText;
    private int _currentIndex;

    public TypingSession(string targetText)
    {
        _typingText = targetText;
    }

    public bool ValidateTextInput(char input)
    {
        if (IsCompleted) return false;

        bool correct = input == TypingText[CurrentIndex];
        if (correct)
        {
            _currentIndex++;
        }
        return correct;
    }
}
