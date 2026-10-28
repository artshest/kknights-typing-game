using KK.Core.Typing;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Results
{
    public class ResultsView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _speed;
        [SerializeField]
        private TextMeshProUGUI _accuracy;
        [SerializeField]
        private TextMeshProUGUI _mistakes;
        [SerializeField]
        private TextMeshProUGUI _duration;
        [SerializeField]
        private Button _restartButton;
        [SerializeField]
        private Button _nextTextButton;

        public event Action RestartRequested;
        public event Action NextTextRequested;

        public void Show(TypingResult result)
        {
            _speed.text = ResultsFormatter.FormatSpeed(result.CharactersPerMinute);
            _accuracy.text = ResultsFormatter.FormatAccuracy(result.Accuracy);
            _mistakes.text = ResultsFormatter.FormatMistakes(result.Mistakes);
            _duration.text = ResultsFormatter.FormatDuration(result.Duration);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(HandleRestartClicked);
            _nextTextButton.onClick.AddListener(HandleNextTextClicked);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
            _nextTextButton.onClick.RemoveListener(HandleNextTextClicked);
        }

        private void HandleRestartClicked()
        {
            RestartRequested?.Invoke();
        }

        private void HandleNextTextClicked()
        {
            NextTextRequested?.Invoke();
        }
    }
}
