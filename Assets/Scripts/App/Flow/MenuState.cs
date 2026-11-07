using KK.App.Flow.Screens;
using KK.App.Texts;
using System;

namespace KK.App.Flow
{
    public class MenuState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IMenuScreen _screen;
        private readonly IArticleLinkValidator _linkValidator;

        // Живёт дольше одного входа: вернувшись в меню, игрок видит свою прошлую ссылку, а не ссылку по умолчанию
        private string _lastLink;

        public MenuState(GameStateMachine stateMachine, IMenuScreen screen, IArticleLinkValidator linkValidator, string defaultLink)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _linkValidator = linkValidator ?? throw new ArgumentNullException(nameof(linkValidator));
            // Ссылки по умолчанию может не быть — тогда поле просто пустое
            _lastLink = defaultLink ?? string.Empty;
        }

        public void Enter()
        {
            _screen.LibrarySelected += HandleLibrarySelected;
            _screen.ArticleSelected += HandleArticleSelected;
            _screen.Show(_lastLink);
        }

        public void Exit()
        {
            _screen.LibrarySelected -= HandleLibrarySelected;
            _screen.ArticleSelected -= HandleArticleSelected;
            _screen.Hide();
        }

        private void HandleLibrarySelected()
        {
            _stateMachine.Enter<LoadingState, TextSource>(TextSource.Library());
        }

        private void HandleArticleSelected(string link)
        {
            // Запоминаем и неверную ссылку: игрок исправит её, а не наберёт заново
            _lastLink = link ?? string.Empty;

            if (!_linkValidator.IsValid(link))
            {
                _screen.ShowLinkError();
                return;
            }

            _stateMachine.Enter<LoadingState, TextSource>(TextSource.Article(link));
        }
    }
}
