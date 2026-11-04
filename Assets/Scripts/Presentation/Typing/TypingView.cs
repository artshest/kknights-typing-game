using KK.App.Flow.Screens;
using KK.App.Timing;
using KK.App.Typing;
using KK.Core.Typing;
using R3;
using System;
using TMPro;
using UnityEngine;

namespace KK.Presentation.Typing
{
    public class TypingView : MonoBehaviour, ITypingScreen
    {
        [SerializeField]
        private TextMeshProUGUI _typingTextMesh;

        [Space]
        [Header("Stats")]
        [SerializeField]
        private TextMeshProUGUI _total;
        [SerializeField]
        private TextMeshProUGUI _speed;
        [SerializeField]
        private TextMeshProUGUI _totalMistakes;

        public event Action<TypingResult> Completed;

        private ITypingInput _typingInput;
        private IClock _clock;
        private TypingViewModel _viewModel;
        private DisposableBag _disposables;

        // TODO: ввод и часы уйдут в TypingState вместе с логикой раунда (claude/TODO/typing-round-to-app-layer.md)
        public void Construct(ITypingInput typingInput, IClock clock)
        {
            _typingInput = typingInput ?? throw new ArgumentNullException(nameof(typingInput));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void Begin(TypingSession session)
        {
            if (_typingInput == null)
                throw new InvalidOperationException($"Перед {nameof(Begin)} нужно вызвать {nameof(Construct)}");

            End();

            _viewModel = new TypingViewModel(_typingInput, session, _clock);
            _viewModel.Completed += HandleCompleted;

            _viewModel.CurrentIndex
                .CombineLatest(_viewModel.HasMistake, (index, hasMistake) => TypingTextFormatter.Format(_viewModel.TypingText, index, hasMistake))
                .Subscribe(text => _typingTextMesh.text = text)
                .AddTo(ref _disposables);
            _viewModel.Stats.Subscribe(ShowStats).AddTo(ref _disposables);
        }

        public void End()
        {
            if (_viewModel == null)
                return;

            // Освобождённый DisposableBag сразу освобождает всё, что в него добавят, поэтому нужен новый
            _disposables.Dispose();
            _disposables = new DisposableBag();

            _viewModel.Completed -= HandleCompleted;
            _viewModel.Dispose();
            _viewModel = null;
        }

        private void Update()
        {
            // Скорость падает и во время паузы игрока, поэтому пересчёт идёт каждый кадр, а не только по нажатию
            _viewModel?.Tick();
        }

        private void HandleCompleted(TypingResult result)
        {
            Completed?.Invoke(result);
        }

        private void ShowStats(TypingResult stats)
        {
            _total.text = stats.CorrectCount.ToString();
            _speed.text = stats.CharactersPerMinute.ToString("F0");
            _totalMistakes.text = stats.Mistakes.ToString();
        }

        private void OnDestroy()
        {
            End();
        }
    }
}
