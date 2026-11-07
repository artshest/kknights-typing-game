using KK.App.Flow.Screens;
using KK.App.Typing;
using KK.Core.Typing;
using System;

namespace KK.App.Flow
{
    public class TypingState : IPayloadState<LoadedText>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ITypingScreen _screen;
        private readonly ITypingInputFactory _inputFactory;

        private LoadedText _loadedText;
        private ITypingInput _input;

        public TypingState(GameStateMachine stateMachine, ITypingScreen screen, ITypingInputFactory inputFactory)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _inputFactory = inputFactory ?? throw new ArgumentNullException(nameof(inputFactory));
        }

        public void Enter(LoadedText loadedText)
        {
            _loadedText = loadedText;
            var session = new TypingSession(loadedText.Text);

            _input = _inputFactory.Create()
                ?? throw new InvalidOperationException($"{nameof(ITypingInputFactory)} вернула null");

            _screen.Completed += HandleCompleted;
            _screen.Begin(session, _input);
        }

        public void Exit()
        {
            _screen.Completed -= HandleCompleted;
            // Сначала экран: ViewModel отпишется от ввода, и только потом ввод освобождается
            _screen.End();
            _input?.Dispose();
            _input = null;
        }

        private void HandleCompleted(TypingResult result)
        {
            _stateMachine.Enter<ResultsState, RoundResult>(new RoundResult(result, _loadedText));
        }
    }
}
