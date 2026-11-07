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
        [SerializeField]
        private TypingCaret _caret;
        [SerializeField]
        private KeyboardView _keyboard;
        [SerializeField]
        private SegmentedProgressBar _progressBar;

        [Space]
        [Header("Stats")]
        [SerializeField]
        private TextMeshProUGUI _typedCount;
        [SerializeField]
        private TextMeshProUGUI _speed;
        [SerializeField]
        private TextMeshProUGUI _accuracy;
        [SerializeField]
        private TextMeshProUGUI _totalMistakes;
        [SerializeField]
        private TextMeshProUGUI _duration;

        public event Action<TypingResult> Completed;

        private IClock _clock;
        private TypingViewModel _viewModel;
        private DisposableBag _disposables;

        // TODO: часы уйдут в TypingState вместе с логикой раунда (claude/TODO/typing-round-to-app-layer.md)
        public void Construct(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void Begin(TypingSession session, ITypingInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (_clock == null)
                throw new InvalidOperationException($"Перед {nameof(Begin)} нужно вызвать {nameof(Construct)}");

            End();

            _viewModel = new TypingViewModel(input, session, _clock);
            _viewModel.Completed += HandleCompleted;

            _viewModel.CurrentIndex
                .CombineLatest(_viewModel.HasMistake, (index, hasMistake) => (index, hasMistake))
                .Subscribe(state => ShowProgress(state.index, state.hasMistake))
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

            // Набранный текст остаётся фоном, а подсказки «что жать дальше» после раунда уже лишние.
            // Проверки на null: при выгрузке сцены дочерние объекты могут быть уничтожены раньше
            if (_caret != null)
                _caret.Hide();
            if (_keyboard != null)
                _keyboard.Clear();
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

        private void ShowProgress(int index, bool hasMistake)
        {
            string text = _viewModel.TypingText;
            _typingTextMesh.text = TypingTextFormatter.Format(text, index, hasMistake);
            _typedCount.text = TypingStatsFormatter.FormatTypedCount(index, text.Length);
            _progressBar.SetProgress(text.Length == 0 ? 1f : (float)index / text.Length);

            if (index < text.Length)
            {
                _caret.Show(index, hasMistake);
                _keyboard.Highlight(text[index], hasMistake);
            }
            else
            {
                _caret.Hide();
                _keyboard.Clear();
            }
        }

        private void ShowStats(TypingResult stats)
        {
            _speed.text = TypingStatsFormatter.FormatSpeed(stats.CharactersPerMinute);
            _accuracy.text = TypingStatsFormatter.FormatAccuracy(stats.Accuracy);
            _totalMistakes.text = TypingStatsFormatter.FormatMistakes(stats.Mistakes);
            _duration.text = TypingStatsFormatter.FormatSeconds(stats.Duration);
        }

        private void OnDestroy()
        {
            End();
        }
    }
}
