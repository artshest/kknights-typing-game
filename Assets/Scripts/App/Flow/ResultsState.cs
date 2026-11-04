using KK.App.Flow.Screens;
using System;

namespace KK.App.Flow
{
    public class ResultsState : IPayloadState<RoundResult>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IResultsScreen _screen;

        private string _text;

        public ResultsState(GameStateMachine stateMachine, IResultsScreen screen)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public void Enter(RoundResult round)
        {
            _text = round.Text;
            _screen.RestartRequested += HandleRestartRequested;
            _screen.NextTextRequested += HandleNextTextRequested;
            _screen.Show(round.Result);
        }

        public void Exit()
        {
            _screen.RestartRequested -= HandleRestartRequested;
            _screen.NextTextRequested -= HandleNextTextRequested;
            _screen.Hide();
        }

        private void HandleRestartRequested()
        {
            _stateMachine.Enter<TypingState, string>(_text);
        }

        private void HandleNextTextRequested()
        {
            _stateMachine.Enter<LoadingState>();
        }
    }
}
