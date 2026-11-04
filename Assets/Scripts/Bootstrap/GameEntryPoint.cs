using KK.App.Flow;
using KK.App.Typing;
using KK.Core.Texts;
using KK.Infrastructure.Input;
using KK.Infrastructure.Logging;
using KK.Infrastructure.Timing;
using KK.Presentation.Loading;
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
        private TypingView _typingView;
        [SerializeField]
        private ResultsView _resultsView;
        [SerializeField]
        private LoadStatusView _loadStatusView;

        [Header("Normalization")]
        [SerializeField]
        // TODO: убрать, когда появятся настройки игрока
        [Tooltip("Заменять «ё» на «е»")]
        private bool _replaceYo = true;

        private ITypingInput _typingInput;
        private GameStateMachine _stateMachine;

        private void Start()
        {
            _resultsView.Hide();
            _loadStatusView.Hide();

            // Ввод живёт, пока жива точка входа; на каждый раунд пересоздаются только сессия и ViewModel
            _typingInput = new TextInputHandler();
            _typingView.Construct(_typingInput, new UnityClock());

            _stateMachine = new GameStateMachine();
            _stateMachine.Register(new LoadingState(_stateMachine, _loadStatusView, _providerFactory,
                new TextNormalizationOptions { ReplaceYo = _replaceYo }, new UnityAppLogger()));
            _stateMachine.Register(new TypingState(_stateMachine, _typingView));
            _stateMachine.Register(new ResultsState(_stateMachine, _resultsView));

            _stateMachine.Enter<LoadingState>();
        }

        private void OnDestroy()
        {
            // Выход из текущего состояния отменит загрузку или освободит ViewModel раунда
            _stateMachine?.Exit();
            // Ввод создала точка входа, ей и освобождать
            _typingInput?.Dispose();
        }
    }
}
