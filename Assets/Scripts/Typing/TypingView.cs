using NUnit.Framework;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypingView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _targetTextMesh;

    [SerializeField]
    private TextMeshProUGUI _remainTextMesh;
    
    private TypingViewModel _typingViewModel;
    private DisposableBag _disposables;

    public void Bind(TypingViewModel model)
    {
        model.TargetText.Subscribe(x => _targetTextMesh.text = x).AddTo(ref _disposables);
        model.RemainingText.Subscribe(x => _remainTextMesh.text = x).AddTo(ref _disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
