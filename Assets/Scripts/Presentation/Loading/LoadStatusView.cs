using KK.App.Texts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Loading
{
    public class LoadStatusView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _message;
        [SerializeField]
        private Button _retryButton;

        public event Action RetryRequested;

        public void ShowLoading()
        {
            // Без кнопки: повторный запрос, пока идёт текущий, запустил бы вторую загрузку параллельно
            Show(LoadErrorFormatter.Loading, canRetry: false);
        }

        public void ShowError(TextLoadErrorReason reason)
        {
            Show(LoadErrorFormatter.Format(reason), canRetry: true);
        }

        public void ShowUnexpectedError()
        {
            Show(LoadErrorFormatter.Unexpected, canRetry: true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Show(string message, bool canRetry)
        {
            _message.text = message;
            _retryButton.gameObject.SetActive(canRetry);
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(HandleRetryClicked);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        private void HandleRetryClicked()
        {
            RetryRequested?.Invoke();
        }
    }
}
