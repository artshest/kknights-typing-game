using Cysharp.Threading.Tasks;
using KK.App.Timing;
using KK.App.Typing;
using KK.Core.Texts;
using KK.Core.Typing;
using KK.Infrastructure.Input;
using KK.Infrastructure.Timing;
using KK.Presentation.Results;
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
        [SerializeField]
        private ResultsView _resultsView;

        [Header("Normalization")]
        [SerializeField]
        // TODO: убрать, когда появятся настройки игрока
        [Tooltip("Заменять «ё» на «е»")]
        private bool _replaceYo = true;

        private ITypingInput _typingInput;
        private IClock _clock;
        private string _text;
        private TypingViewModel _typingViewModel;

        private void Start()
        {
            _resultsView.Hide();
            _resultsView.RestartRequested += HandleRestartRequested;
            _resultsView.NextTextRequested += HandleNextTextRequested;

            // Ввод и часы живут, пока жив фасад; на каждый раунд пересоздаются только сессия и ViewModel
            _typingInput = new TextInputHandler();
            _clock = new UnityClock();

            StartSessionWithNewTextAsync().Forget();
        }

        private void Update()
        {
            // Скорость падает и во время паузы игрока, поэтому пересчёт идёт каждый кадр, а не только по нажатию
            _typingViewModel?.Tick();
        }

        private async UniTaskVoid StartSessionWithNewTextAsync()
        {
            try
            {
                string rawText = await _providerFactory.Create().GetTextAsync(destroyCancellationToken);
                _text = TextNormalizer.Normalize(rawText, new TextNormalizationOptions { ReplaceYo = _replaceYo });

                StartSession();
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

        private void StartSession()
        {
            _typingViewModel = new TypingViewModel(_typingInput, new TypingSession(_text), _clock);
            _typingViewModel.Completed += HandleCompleted;
            _typingView.Bind(_typingViewModel);
        }

        private void EndSession()
        {
            _resultsView.Hide();
            _typingViewModel.Dispose();
            // Пока грузится новый текст, Update не должен трогать освобождённую ViewModel
            _typingViewModel = null;
        }

        private void HandleCompleted(TypingResult result)
        {
            _resultsView.Show(result);
        }

        private void HandleRestartRequested()
        {
            EndSession();
            StartSession();
        }

        private void HandleNextTextRequested()
        {
            EndSession();
            StartSessionWithNewTextAsync().Forget();
        }

        private void OnDestroy()
        {
            _resultsView.RestartRequested -= HandleRestartRequested;
            _resultsView.NextTextRequested -= HandleNextTextRequested;
            _typingViewModel?.Dispose();
            _typingInput?.Dispose();
        }
    }
}
