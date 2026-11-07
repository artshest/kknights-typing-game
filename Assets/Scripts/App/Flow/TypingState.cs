using KK.App.Flow.Screens;
using KK.Core.Typing;
using System;

namespace KK.App.Flow
{
    public class TypingState : IPayloadState<LoadedText>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ITypingScreen _screen;

        private LoadedText _loadedText;

        public TypingState(GameStateMachine stateMachine, ITypingScreen screen)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public void Enter(LoadedText loadedText)
        {
            _loadedText = loadedText;
            _screen.Completed += HandleCompleted;
            _screen.Begin(new TypingSession(loadedText.Text));
        }

        public void Exit()
        {
            _screen.Completed -= HandleCompleted;
            _screen.End();
        }

        private void HandleCompleted(TypingResult result)
        {
            _stateMachine.Enter<ResultsState, RoundResult>(new RoundResult(result, _loadedText));
        }
    }
}
