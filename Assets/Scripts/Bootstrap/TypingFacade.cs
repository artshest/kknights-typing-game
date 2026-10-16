using System;
using UnityEngine;

public class TypingFacade : MonoBehaviour
{
    [SerializeField]
    private TypingTextProviderFactory _providerFactory;
    [SerializeField]
    private TypingView _typingView;

    private ITypingInput _typingInput;
    private TypingSession _typingSession;
    private TypingViewModel _typingViewModel;

    private async void Start()
    {
        try
        {
            ITypingTextProvider provider = _providerFactory.Create();
            string text = await provider.GetTextAsync();

            _typingInput = new TextInputHandler();

            _typingSession = new TypingSession(text);
            _typingViewModel = new TypingViewModel(_typingInput, _typingSession);
            _typingView.Bind(_typingViewModel);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

    }
}
