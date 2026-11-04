using KK.App.Flow.Screens;
using KK.Core.Typing;
using System;

namespace KK.App.Flow
{
    public class TypingState : IPayloadState<string>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ITypingScreen _screen;

        private string _text;

        public TypingState(GameStateMachine stateMachine, ITypingScreen screen)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public void Enter(string text)
        {
            _text = text;
            _screen.Completed += HandleCompleted;
            _screen.Begin(new TypingSession(text));
        }

        public void Exit()
        {
            _screen.Completed -= HandleCompleted;
            _screen.End();
        }

        private void HandleCompleted(TypingResult result)
        {
            _stateMachine.Enter<ResultsState, RoundResult>(new RoundResult(result, _text));
        }
    }
}
