using KK.Core.Typing;
using R3;
using TMPro;
using UnityEngine;

namespace KK.Presentation.Typing
{
    public class TypingView : MonoBehaviour
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

        private DisposableBag _disposables;

        public void Bind(TypingViewModel model)
        {
            // Повторный Bind (например, новая тренировка) не должен копить старые подписки
            _disposables.Dispose();
            _disposables = new DisposableBag();

            model.CurrentIndex
                .CombineLatest(model.HasMistake, (index, hasMistake) => TypingTextFormatter.Format(model.TypingText, index, hasMistake))
                .Subscribe(text => _typingTextMesh.text = text)
                .AddTo(ref _disposables);
            model.Stats.Subscribe(ShowStats).AddTo(ref _disposables);
        }

        private void ShowStats(TypingResult stats)
        {
            _total.text = stats.CorrectCount.ToString();
            _speed.text = stats.CharactersPerMinute.ToString("F0");
            _totalMistakes.text = stats.Mistakes.ToString();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
