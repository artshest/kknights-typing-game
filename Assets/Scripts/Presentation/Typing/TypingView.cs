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
        private TextMeshProUGUI _speed;
        private TextMeshProUGUI _totalMistakes;

        private static string _alphaTagOpen = "<color=\"white\"><alpha=#33>";
        private static string _alphaTagClose = "<color=#FF9E4A><alpha=#FF>";

        private TypingViewModel _typingViewModel;
        private DisposableBag _disposables;

        public void Bind(TypingViewModel model)
        {
            model.CurrentIndex.Subscribe(x => ConvertTextToView(model.TypingText, x)).AddTo(ref _disposables);
            model.CurrentIndex.Subscribe(x => _total.text = x.ToString()).AddTo(ref _disposables);
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
}
