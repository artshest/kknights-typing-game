using System;
using KK.App.Flow;
using KK.App.Tests.Fakes;
using KK.App.Texts;
using NUnit.Framework;

namespace KK.App.Tests.Flow
{
    public class MenuStateTests
    {
        private const string InvalidLink = "просто текст";

        private FlowTestContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new FlowTestContext();
        }

        [Test]
        public void Enter_ShowsDefaultLink()
        {
            _context.Machine.Enter<MenuState>();

            Assert.That(_context.MenuScreen.IsShown, Is.True);
            Assert.That(_context.MenuScreen.ShownLink, Is.EqualTo(FlowTestContext.DefaultLink));
        }

        [Test]
        public void Enter_NoDefaultLink_ShowsEmptyField()
        {
            var machine = new GameStateMachine();
            var screen = new FakeMenuScreen();
            machine.Register(new MenuState(machine, screen, new FakeArticleLinkValidator(), null));

            machine.Enter<MenuState>();

            Assert.That(screen.ShownLink, Is.Empty);
        }

        [Test]
        public void LibrarySelected_LoadsFromLibrary()
        {
            _context.Machine.Enter<MenuState>();

            _context.MenuScreen.ClickLibrary();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
            Assert.That(_context.ProviderFactory.RequestedSources, Is.EqualTo(new[] { TextSource.Library() }));
            Assert.That(_context.MenuScreen.IsShown, Is.False);
            Assert.That(_context.MenuScreen.HasSubscribers, Is.False);
        }

        [Test]
        public void ValidArticleSelected_LoadsThatArticle()
        {
            _context.Machine.Enter<MenuState>();

            _context.MenuScreen.ClickArticle(FakeArticleLinkValidator.ValidLink);

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
            Assert.That(_context.ProviderFactory.RequestedSources,
                Is.EqualTo(new[] { TextSource.Article(FakeArticleLinkValidator.ValidLink) }));
            Assert.That(_context.MenuScreen.LinkErrorCount, Is.Zero);
        }

        [TestCase(InvalidLink)]
        [TestCase("")]
        [TestCase(null)]
        public void InvalidArticleSelected_StaysInMenuWithError(string link)
        {
            _context.Machine.Enter<MenuState>();

            _context.MenuScreen.ClickArticle(link);

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<MenuState>());
            Assert.That(_context.MenuScreen.LinkErrorCount, Is.EqualTo(1));
            Assert.That(_context.MenuScreen.IsShown, Is.True);
            Assert.That(_context.ProviderFactory.RequestedSources, Is.Empty, "Загрузка не должна начинаться");
        }

        [Test]
        public void InvalidLink_ThenValid_Loads()
        {
            // Ошибка не отписывает меню от событий: игрок исправляет ссылку и пробует снова
            _context.Machine.Enter<MenuState>();
            _context.MenuScreen.ClickArticle(InvalidLink);

            _context.MenuScreen.ClickArticle(FakeArticleLinkValidator.ValidLink);

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
        }

        [Test]
        public void ReturnToMenu_ShowsLastEnteredLink()
        {
            _context.Machine.Enter<MenuState>();
            _context.MenuScreen.ClickArticle(FakeArticleLinkValidator.ValidLink);

            _context.Machine.Enter<MenuState>();

            Assert.That(_context.MenuScreen.ShownLink, Is.EqualTo(FakeArticleLinkValidator.ValidLink));
        }

        [Test]
        public void ReturnToMenu_AfterInvalidLink_KeepsItForEditing()
        {
            _context.Machine.Enter<MenuState>();
            _context.MenuScreen.ClickArticle(InvalidLink);

            // Повторный вход (Exit + Enter) — как возврат в меню
            _context.Machine.Enter<MenuState>();

            Assert.That(_context.MenuScreen.ShownLink, Is.EqualTo(InvalidLink));
        }

        [Test]
        public void Exit_HidesAndUnsubscribes()
        {
            _context.Machine.Enter<MenuState>();

            _context.Machine.Exit();

            Assert.That(_context.MenuScreen.IsShown, Is.False);
            Assert.That(_context.MenuScreen.HasSubscribers, Is.False);
        }

        [Test]
        public void Constructor_NullDependency_Throws()
        {
            var machine = new GameStateMachine();
            var screen = new FakeMenuScreen();
            var validator = new FakeArticleLinkValidator();

            Assert.Throws<ArgumentNullException>(() => new MenuState(null, screen, validator, ""));
            Assert.Throws<ArgumentNullException>(() => new MenuState(machine, null, validator, ""));
            Assert.Throws<ArgumentNullException>(() => new MenuState(machine, screen, null, ""));
        }
    }
}
