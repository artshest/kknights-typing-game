using KK.App.Flow.Screens;
using KK.App.Texts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Loading
{
    public class LoadStatusView : MonoBehaviour, ILoadingScreen
    {
        [SerializeField]
        private TextMeshProUGUI _message;
        [SerializeField]
        private Button _retryButton;
        [SerializeField]
        private Button _menuButton;

        public event Action RetryRequested;
        public event Action MenuRequested;

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
            // При выгрузке сцены объекты уничтожаются в произвольном порядке: состояние может закрыть экран,
            // который уже уничтожен, а обращение к его gameObject бросит MissingReferenceException
            if (this == null)
                return;

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
            _menuButton.onClick.AddListener(HandleMenuClicked);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(HandleRetryClicked);
            _menuButton.onClick.RemoveListener(HandleMenuClicked);
        }

        private void HandleRetryClicked()
        {
            RetryRequested?.Invoke();
        }

        private void HandleMenuClicked()
        {
            MenuRequested?.Invoke();
        }
    }
}
