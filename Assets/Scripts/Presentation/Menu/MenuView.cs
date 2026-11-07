using KK.App.Flow.Screens;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Menu
{
    public class MenuView : MonoBehaviour, IMenuScreen
    {
        [SerializeField]
        private Button _libraryButton;
        [SerializeField]
        private TMP_InputField _linkInput;
        [SerializeField]
        private Button _articleButton;
        [SerializeField]
        private TextMeshProUGUI _linkError;

        public event Action LibrarySelected;
        public event Action<string> ArticleSelected;

        public void Show(string link)
        {
            // Без уведомления: заполнение поля кодом — не правка игрока
            _linkInput.SetTextWithoutNotify(link);
            _linkError.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void ShowLinkError()
        {
            _linkError.text = MenuTexts.InvalidLink;
            _linkError.gameObject.SetActive(true);
        }

        public void Hide()
        {
            // При выгрузке сцены объекты уничтожаются в произвольном порядке: состояние может закрыть экран,
            // который уже уничтожен, а обращение к его gameObject бросит MissingReferenceException
            if (this == null)
                return;

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _libraryButton.onClick.AddListener(HandleLibraryClicked);
            _articleButton.onClick.AddListener(HandleArticleClicked);
            _linkInput.onSubmit.AddListener(HandleLinkSubmitted);
            _linkInput.onValueChanged.AddListener(HandleLinkChanged);
        }

        private void OnDisable()
        {
            _libraryButton.onClick.RemoveListener(HandleLibraryClicked);
            _articleButton.onClick.RemoveListener(HandleArticleClicked);
            _linkInput.onSubmit.RemoveListener(HandleLinkSubmitted);
            _linkInput.onValueChanged.RemoveListener(HandleLinkChanged);
        }

        private void HandleLibraryClicked()
        {
            LibrarySelected?.Invoke();
        }

        private void HandleArticleClicked()
        {
            ArticleSelected?.Invoke(_linkInput.text);
        }

        private void HandleLinkSubmitted(string link)
        {
            ArticleSelected?.Invoke(link);
        }

        private void HandleLinkChanged(string _)
        {
            // Игрок начал исправлять ссылку — старая ошибка уже не про неё
            _linkError.gameObject.SetActive(false);
        }
    }
}
