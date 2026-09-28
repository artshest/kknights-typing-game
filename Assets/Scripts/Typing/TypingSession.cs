using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TypingSession
{
    public string TargetText { get => _targetText; }
    public string RemainingText { get => _remainingText; }

    [SerializeField]
    private string _targetText;

    private string _remainingText;

    public TypingSession(string targetText)
    {
        _targetText = targetText;
        _remainingText = _targetText;
    }

    public bool ValidateTextInput(char input, out string remainingText, out bool isComplete)
    {
        isComplete = false;

        if (input != _remainingText.First())
        {
            remainingText = _remainingText;
            return false;
        }

        // Строки иммутабельны, потому каждый такой ремув создают новую строку, что засирает память
        // TODO: подумать, как можно оптимизировать это дело
        _remainingText = _remainingText.Remove(0, 1);

        remainingText = _remainingText;

        if (_remainingText.Length == 0)
        {
            Debug.Log("Текст введен!");

            remainingText = _remainingText;
            isComplete = true;
        }

        return true;
    }
}
