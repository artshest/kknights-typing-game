using KK.App.Flow;
using KK.Core.Texts;
using KK.Infrastructure.Input;
using KK.Infrastructure.Logging;
using KK.Infrastructure.Texts.KKnights;
using KK.Infrastructure.Timing;
using KK.Presentation.Loading;
using KK.Presentation.Menu;
using KK.Presentation.Results;
using KK.Presentation.Typing;
using UnityEngine;

namespace KK.Bootstrap
{
    // Composition root и мост от жизненного цикла Unity к машине состояний. Решений об игре здесь нет:
    // что показать и куда перейти, решают состояния в KK.App
    public class GameEntryPoint : MonoBehaviour
    {
        // Поля — конкретные типы, потому что Unity не сериализует поля-интерфейсы.
        // Состояния получают те же объекты уже через порты
        [SerializeField]
        private TypingTextProviderFactory _providerFactory;
        [SerializeField]
        private MenuView _menuView;
        [SerializeField]
        private TypingView _typingView;
        [SerializeField]
        private ResultsView _resultsView;
        [SerializeField]
        private LoadStatusView _loadStatusView;

        [Header("Menu")]
        [SerializeField]
        [Tooltip("Ссылка, которой заполнено поле меню при запуске, например https://kknights.com/posts/5235-valor-mortis")]
        private string _defaultArticleLink;

        [Header("Normalization")]
        [SerializeField]
        [Tooltip("Заменять «ё» на «е»")]
        private bool _replaceYo = true;

        private GameStateMachine _stateMachine;

        private void Start()
        {
            _menuView.Hide();
            _resultsView.Hide();
            _loadStatusView.Hide();

            _typingView.Construct(new UnityClock());

            _stateMachine = new GameStateMachine();
            _stateMachine.Register(new MenuState(_stateMachine, _menuView, new KKnightsArticleLinkValidator(),
                _defaultArticleLink));
            _stateMachine.Register(new LoadingState(_stateMachine, _loadStatusView, _providerFactory,
                new TextNormalizationOptions { ReplaceYo = _replaceYo }, new UnityAppLogger()));
            _stateMachine.Register(new TypingState(_stateMachine, _typingView, new TextInputHandlerFactory()));
            _stateMachine.Register(new ResultsState(_stateMachine, _resultsView));

            _stateMachine.Enter<MenuState>();
        }

        private void OnDestroy()
        {
            // Выход из текущего состояния отменит загрузку или освободит ввод и ViewModel раунда
            _stateMachine?.Exit();
        }
    }
}
