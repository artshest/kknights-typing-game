using System;
using KK.App.Flow;
using KK.Core.Typing;
using NUnit.Framework;

namespace KK.App.Tests.Flow
{
    public class TypingAndResultsStateTests
    {
        private static readonly TypingResult SomeResult = new(300, 0.95, 2, 40, TimeSpan.FromSeconds(8));

        private FlowTestContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new FlowTestContext();
        }

        [Test]
        public void TypingEnter_BeginsSessionWithText()
        {
            _context.Machine.Enter<TypingState, string>("текст");

            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("текст"));
        }

        [Test]
        public void TypingCompleted_EntersResultsWithResult()
        {
            _context.Machine.Enter<TypingState, string>("текст");

            _context.TypingScreen.Complete(SomeResult);

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<ResultsState>());
            Assert.That(_context.ResultsScreen.ShownResult, Is.EqualTo(SomeResult));
            Assert.That(_context.TypingScreen.Session, Is.Null, "Раунд должен закончиться");
            Assert.That(_context.TypingScreen.HasCompletedSubscribers, Is.False);
        }

        [Test]
        public void Restart_EntersTypingWithSameText()
        {
            _context.Machine.Enter<TypingState, string>("текст");
            _context.TypingScreen.Complete(SomeResult);

            _context.ResultsScreen.ClickRestart();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<TypingState>());
            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("текст"));
            Assert.That(_context.TypingScreen.BeginCount, Is.EqualTo(2));
            Assert.That(_context.ResultsScreen.ShownResult, Is.Null);
            Assert.That(_context.ResultsScreen.HasSubscribers, Is.False);
        }

        [Test]
        public void NextText_EntersLoading()
        {
            _context.Machine.Enter<TypingState, string>("текст");
            _context.TypingScreen.Complete(SomeResult);

            _context.ResultsScreen.ClickNextText();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
            Assert.That(_context.Provider.RequestCount, Is.EqualTo(1));
            Assert.That(_context.ResultsScreen.ShownResult, Is.Null);
        }

        [Test]
        public void FullRound_LoadTypeNextText_TypesSecondText()
        {
            _context.Machine.Enter<LoadingState>();
            _context.Provider.Complete("первый");
            _context.TypingScreen.Complete(SomeResult);
            _context.ResultsScreen.ClickNextText();
            _context.Provider.Complete("второй");

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<TypingState>());
            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("второй"));
        }

        [Test]
        public void Constructors_NullDependency_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new TypingState(null, _context.TypingScreen));
            Assert.Throws<ArgumentNullException>(() => new TypingState(_context.Machine, null));
            Assert.Throws<ArgumentNullException>(() => new ResultsState(null, _context.ResultsScreen));
            Assert.Throws<ArgumentNullException>(() => new ResultsState(_context.Machine, null));
        }
    }
}
