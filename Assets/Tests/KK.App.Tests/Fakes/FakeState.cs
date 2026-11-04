using System.Collections.Generic;
using KK.App.Flow;

namespace KK.App.Tests.Fakes
{
    // Общий журнал на несколько состояний показывает порядок вызовов между ними
    public class FakeState : IState
    {
        private readonly string _name;
        private readonly List<string> _log;

        public FakeState(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public void Enter()
        {
            _log.Add($"{_name}.Enter");
        }

        public void Exit()
        {
            _log.Add($"{_name}.Exit");
        }
    }

    public class OtherFakeState : FakeState
    {
        public OtherFakeState(string name, List<string> log) : base(name, log) { }
    }

    public class FakePayloadState : IPayloadState<int>
    {
        public int? Payload { get; private set; }

        public void Enter(int payload)
        {
            Payload = payload;
        }

        public void Exit()
        {
            Payload = null;
        }
    }
}
