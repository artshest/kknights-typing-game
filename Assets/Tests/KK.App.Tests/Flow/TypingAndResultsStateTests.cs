using System;
using KK.App.Flow;
using KK.App.Tests.Fakes;
using KK.App.Texts;
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
            _context.EnterTyping("текст");

            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("текст"));
        }

        [Test]
        public void TypingEnter_CreatesInputAndPassesToScreen()
        {
            _context.EnterTyping("текст");

            Assert.That(_context.InputFactory.Created, Has.Count.EqualTo(1));
            Assert.That(_context.TypingScreen.Input, Is.SameAs(_context.InputFactory.Last));
            Assert.That(_context.InputFactory.Last.IsDisposed, Is.False);
        }

        [Test]
        public void TypingExit_EndsScreenThenDisposesInput()
        {
            _context.EnterTyping("текст");
            FakeTypingInput input = _context.InputFactory.Last;

            _context.Machine.Exit();

            Assert.That(_context.TypingScreen.InputDisposedAtEnd, Is.False, "Экран заканчивает раунд, пока ввод ещё жив");
            Assert.That(input.IsDisposed, Is.True);
        }

        [Test]
        public void TypingCompleted_DisposesInput()
        {
            _context.EnterTyping("текст");

            _context.TypingScreen.Complete(SomeResult);

            Assert.That(_context.InputFactory.Last.IsDisposed, Is.True);
        }

        [Test]
        public void Restart_CreatesFreshInput()
        {
            _context.EnterTyping("текст");
            FakeTypingInput first = _context.InputFactory.Last;
            _context.TypingScreen.Complete(SomeResult);

            _context.ResultsScreen.ClickRestart();

            Assert.That(_context.InputFactory.Created, Has.Count.EqualTo(2));
            Assert.That(first.IsDisposed, Is.True);
            Assert.That(_context.InputFactory.Last.IsDisposed, Is.False);
            Assert.That(_context.TypingScreen.Input, Is.SameAs(_context.InputFactory.Last));
        }

        [Test]
        public void OutsideTyping_NoInputCreated()
        {
            // Пока игрок вводит ссылку в меню или ждёт загрузку, клавиатуру никто не слушает
            _context.Machine.Enter<MenuState>();
            _context.MenuScreen.ClickLibrary();

            Assert.That(_context.InputFactory.Created, Is.Empty);
        }

        [Test]
        public void TypingEnter_FactoryReturnsNull_Throws()
        {
            _context.InputFactory.ReturnNull = true;

            Assert.Throws<InvalidOperationException>(() => _context.EnterTyping("текст"));
        }

        [Test]
        public void TypingCompleted_EntersResultsWithResult()
        {
            _context.EnterTyping("текст");

            _context.TypingScreen.Complete(SomeResult);

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<ResultsState>());
            Assert.That(_context.ResultsScreen.ShownResult, Is.EqualTo(SomeResult));
            Assert.That(_context.TypingScreen.Session, Is.Null, "Раунд должен закончиться");
            Assert.That(_context.TypingScreen.HasCompletedSubscribers, Is.False);
        }

        [Test]
        public void Restart_EntersTypingWithSameText()
        {
            _context.EnterTyping("текст");
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
            _context.EnterTyping("текст");
            _context.TypingScreen.Complete(SomeResult);

            _context.ResultsScreen.ClickNextText();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
            Assert.That(_context.Provider.RequestCount, Is.EqualTo(1));
            Assert.That(_context.ProviderFactory.RequestedSources, Is.EqualTo(new[] { FlowTestContext.Source }),
                "Другой текст — из того же источника");
            Assert.That(_context.ResultsScreen.ShownResult, Is.Null);
        }

        [Test]
        public void MenuRequested_EntersMenu()
        {
            _context.EnterTyping("текст");
            _context.TypingScreen.Complete(SomeResult);

            _context.ResultsScreen.ClickMenu();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<MenuState>());
            Assert.That(_context.ResultsScreen.ShownResult, Is.Null);
            Assert.That(_context.ResultsScreen.HasSubscribers, Is.False);
            Assert.That(_context.MenuScreen.IsShown, Is.True);
        }

        [Test]
        public void FullCircle_MenuLibraryResultsMenuArticle_LoadsArticle()
        {
            _context.Machine.Enter<MenuState>();
            _context.MenuScreen.ClickLibrary();
            _context.Provider.Complete("первый");
            _context.TypingScreen.Complete(SomeResult);
            _context.ResultsScreen.ClickMenu();

            _context.MenuScreen.ClickArticle(FakeArticleLinkValidator.ValidLink);
            _context.Provider.Complete("второй");

            Assert.That(_context.ProviderFactory.RequestedSources, Is.EqualTo(new[]
            {
                TextSource.Library(),
                TextSource.Article(FakeArticleLinkValidator.ValidLink)
            }));
            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<TypingState>());
            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("второй"));
        }

        [Test]
        public void FullRound_LoadTypeNextText_TypesSecondText()
        {
            _context.EnterLoading();
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
            Assert.Throws<ArgumentNullException>(() => new TypingState(null, _context.TypingScreen, _context.InputFactory));
            Assert.Throws<ArgumentNullException>(() => new TypingState(_context.Machine, null, _context.InputFactory));
            Assert.Throws<ArgumentNullException>(() => new TypingState(_context.Machine, _context.TypingScreen, null));
            Assert.Throws<ArgumentNullException>(() => new ResultsState(null, _context.ResultsScreen));
            Assert.Throws<ArgumentNullException>(() => new ResultsState(_context.Machine, null));
        }
    }
}
