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

        private const string AlphaTagOpen = "<color=\"white\"><alpha=#33>";
        private const string AlphaTagClose = "<color=#FF9E4A><alpha=#FF>";

        private DisposableBag _disposables;

        public void Bind(TypingViewModel model)
        {
            // Повторный Bind (например, новая тренировка) не должен копить старые подписки
            _disposables.Dispose();
            _disposables = new DisposableBag();

            model.CurrentIndex.Subscribe(x => ConvertTextToView(model.TypingText, x)).AddTo(ref _disposables);
            model.CurrentIndex.Subscribe(x => _total.text = x.ToString()).AddTo(ref _disposables);
        }

        public void ConvertTextToView(string typingText, int currentIndex)
        {
            _typingTextMesh.text = AlphaTagOpen + typingText.Insert(currentIndex, AlphaTagClose);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
