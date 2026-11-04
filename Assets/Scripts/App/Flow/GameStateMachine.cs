using System;
using System.Collections.Generic;

namespace KK.App.Flow
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states = new();
        private IExitableState _currentState;

        public IExitableState CurrentState => _currentState;

        // Регистрация отдельно от конструктора: состояниям нужна сама машина, чтобы переходить дальше,
        // поэтому машина создаётся первой
        public void Register<TState>(TState state) where TState : class, IExitableState
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (_states.ContainsKey(typeof(TState)))
                throw new InvalidOperationException($"Состояние {typeof(TState).Name} уже зарегистрировано");

            _states.Add(typeof(TState), state);
        }

        public void Enter<TState>() where TState : class, IState
        {
            ChangeState<TState>().Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
        {
            ChangeState<TState>().Enter(payload);
        }

        public void Exit()
        {
            IExitableState state = _currentState;
            _currentState = null;
            state?.Exit();
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            // Сначала ищем новое состояние: если его забыли зарегистрировать, текущее не должно остаться
            // наполовину выключенным
            if (!_states.TryGetValue(typeof(TState), out IExitableState next))
                throw new InvalidOperationException($"Состояние {typeof(TState).Name} не зарегистрировано");

            _currentState?.Exit();
            _currentState = next;
            return (TState)next;
        }
    }
}
