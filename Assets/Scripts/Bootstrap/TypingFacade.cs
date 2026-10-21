using KK.App.Texts;
using KK.App.Timing;
using KK.App.Typing;
using KK.Core.Texts;
using KK.Core.Typing;
using KK.Infrastructure.Input;
using KK.Infrastructure.Timing;
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

        [Header("Normalization")]
        [SerializeField]
        // TODO: убрать, когда появятся настройки игрока
        [Tooltip("Заменять «ё» на «е»")]
        private bool _replaceYo = true;

        private ITypingInput _typingInput;
        private TypingSession _typingSession;
        private TypingViewModel _typingViewModel;

        private async void Start()
        {
            try
            {
                ITypingTextProvider provider = _providerFactory.Create();
                string rawText = await provider.GetTextAsync(destroyCancellationToken);
                string text = TextNormalizer.Normalize(rawText, new TextNormalizationOptions { ReplaceYo = _replaceYo });

                _typingInput = new TextInputHandler();
                IClock clock = new UnityClock();

                _typingSession = new TypingSession(text);
                _typingViewModel = new TypingViewModel(_typingInput, _typingSession, clock);
                _typingViewModel.Completed += HandleCompleted;
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

        private void Update()
        {
            // Скорость падает и во время паузы игрока, поэтому пересчёт идёт каждый кадр, а не только по нажатию
            _typingViewModel?.Tick();
        }

        // TODO: заменить экраном результатов (этап 4)
        private void HandleCompleted(TypingResult result)
        {
            Debug.Log($"Результат: {result}");
        }

        private void OnDestroy()
        {
            _typingViewModel?.Dispose();
            _typingInput?.Dispose();
        }
    }
}
