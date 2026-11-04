using System;
using System.Collections.Generic;
using KK.App.Flow;
using KK.App.Tests.Fakes;
using NUnit.Framework;

namespace KK.App.Tests.Flow
{
    public class GameStateMachineTests
    {
        private List<string> _log;
        private GameStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _machine = new GameStateMachine();
            _machine.Register(new FakeState("A", _log));
            _machine.Register(new OtherFakeState("B", _log));
        }

        [Test]
        public void Enter_FirstState_EntersIt()
        {
            _machine.Enter<FakeState>();

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter" }));
            Assert.That(_machine.CurrentState, Is.InstanceOf<FakeState>());
        }

        [Test]
        public void Enter_AnotherState_ExitsPreviousBeforeEntering()
        {
            _machine.Enter<FakeState>();

            _machine.Enter<OtherFakeState>();

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "B.Enter" }));
        }

        [Test]
        public void Enter_SameState_ExitsAndEntersAgain()
        {
            _machine.Enter<FakeState>();

            _machine.Enter<FakeState>();

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "A.Enter" }));
        }

        [Test]
        public void Enter_WithPayload_PassesPayload()
        {
            var state = new FakePayloadState();
            _machine.Register(state);

            _machine.Enter<FakePayloadState, int>(42);

            Assert.That(state.Payload, Is.EqualTo(42));
        }

        [Test]
        public void Enter_UnregisteredState_ThrowsAndKeepsCurrent()
        {
            _machine.Enter<FakeState>();

            Assert.Throws<InvalidOperationException>(() => _machine.Enter<FakePayloadState, int>(1));

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter" }));
            Assert.That(_machine.CurrentState, Is.InstanceOf<FakeState>());
        }

        [Test]
        public void Register_SameTypeTwice_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _machine.Register(new FakeState("A2", _log)));
        }

        [Test]
        public void Register_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.Register<FakeState>(null));
        }

        [Test]
        public void Exit_ExitsCurrentOnce()
        {
            _machine.Enter<FakeState>();

            _machine.Exit();
            _machine.Exit();

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit" }));
            Assert.That(_machine.CurrentState, Is.Null);
        }

        [Test]
        public void Exit_NothingEntered_DoesNothing()
        {
            Assert.DoesNotThrow(() => _machine.Exit());
            Assert.That(_log, Is.Empty);
        }
    }
}
