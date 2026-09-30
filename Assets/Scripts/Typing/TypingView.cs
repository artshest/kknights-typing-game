using NUnit.Framework;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypingView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _typingTextMesh;

    private static string _alphaTagOpen = "<alpha=#88>";
    private static string _alphaTagClose = "<alpha=#FF>";

    private TypingViewModel _typingViewModel;
    private DisposableBag _disposables;

    public void Bind(TypingViewModel model)
    {
        model.CurrentIndex.Subscribe(x => ConvertTextToView(model.TypingText.CurrentValue, x)).AddTo(ref _disposables);
    }

    public void ConvertTextToView(string typingText, int currentIndex)
    {
        _typingTextMesh.text = _alphaTagOpen + typingText.Insert(currentIndex, _alphaTagClose);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
