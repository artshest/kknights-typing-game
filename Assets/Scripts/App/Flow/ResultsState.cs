using KK.App.Flow.Screens;
using KK.App.Texts;
using System;

namespace KK.App.Flow
{
    public class ResultsState : IPayloadState<RoundResult>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IResultsScreen _screen;

        private LoadedText _loadedText;

        public ResultsState(GameStateMachine stateMachine, IResultsScreen screen)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public void Enter(RoundResult round)
        {
            _loadedText = round.LoadedText;
            _screen.RestartRequested += HandleRestartRequested;
            _screen.NextTextRequested += HandleNextTextRequested;
            _screen.MenuRequested += HandleMenuRequested;
            _screen.Show(round.Result);
        }

        public void Exit()
        {
            _screen.RestartRequested -= HandleRestartRequested;
            _screen.NextTextRequested -= HandleNextTextRequested;
            _screen.MenuRequested -= HandleMenuRequested;
            _screen.Hide();
        }

        private void HandleRestartRequested()
        {
            _stateMachine.Enter<TypingState, LoadedText>(_loadedText);
        }

        private void HandleNextTextRequested()
        {
            _stateMachine.Enter<LoadingState, TextSource>(_loadedText.Source);
        }

        private void HandleMenuRequested()
        {
            _stateMachine.Enter<MenuState>();
        }
    }
}
