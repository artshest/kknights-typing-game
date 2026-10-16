using KK.App.Texts;
using KK.App.Typing;
using KK.Core.Typing;
using KK.Infrastructure.Input;
using KK.Presentation.Typing;
using System;
using UnityEngine;

namespace KK.Bootstrap
{
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
                string text = await provider.GetTextAsync(destroyCancellationToken);

                _typingInput = new TextInputHandler();

                _typingSession = new TypingSession(text);
                _typingViewModel = new TypingViewModel(_typingInput, _typingSession);
                _typingView.Bind(_typingViewModel);
            }
            catch (OperationCanceledException)
            {
                // Объект уничтожили во время загрузки — это штатная ситуация, не ошибка
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnDestroy()
        {
            _typingViewModel?.Dispose();
            _typingInput?.Dispose();
        }
    }
}
