using System;
using System.IO;
using KK.App.Flow;
using KK.App.Tests.Fakes;
using KK.App.Texts;
using KK.Core.Texts;
using NUnit.Framework;

namespace KK.App.Tests.Flow
{
    public class LoadingStateTests
    {
        private FlowTestContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new FlowTestContext();
        }

        [Test]
        public void Enter_ShowsLoadingAndRequestsText()
        {
            _context.EnterLoading();

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Loading));
            Assert.That(_context.Provider.RequestCount, Is.EqualTo(1));
        }

        [Test]
        public void Enter_RequestsProviderForGivenSource()
        {
            _context.EnterLoading();

            Assert.That(_context.ProviderFactory.RequestedSources, Is.EqualTo(new[] { FlowTestContext.Source }));
        }

        [Test]
        public void Enter_NullSource_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _context.Machine.Enter<LoadingState, TextSource>(null));
        }

        [Test]
        public void TextLoaded_EntersTypingWithNormalizedText()
        {
            _context.EnterLoading();

            _context.Provider.Complete("  Ёж\nи  ёлка  ");

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<TypingState>());
            Assert.That(_context.TypingScreen.Session.TypingText, Is.EqualTo("Еж и елка"));
            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Hidden));
        }

        [Test]
        public void TextLoaded_ReplaceYoOff_KeepsYo()
        {
            var context = new FlowTestContext(replaceYo: false);
            context.EnterLoading();

            context.Provider.Complete("ёж");

            Assert.That(context.TypingScreen.Session.TypingText, Is.EqualTo("ёж"));
        }

        [TestCase(TextLoadErrorReason.Network)]
        [TestCase(TextLoadErrorReason.NotFound)]
        [TestCase(TextLoadErrorReason.Timeout)]
        [TestCase(TextLoadErrorReason.BadFormat)]
        public void LoadFails_ShowsReasonAndLogsWarning(TextLoadErrorReason reason)
        {
            _context.EnterLoading();

            _context.Provider.Fail(new TextLoadException(reason, "тест"));

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Error));
            Assert.That(_context.LoadingScreen.ErrorReason, Is.EqualTo(reason));
            Assert.That(_context.Logger.Warnings, Has.Count.EqualTo(1));
            Assert.That(_context.Logger.Exceptions, Is.Empty);
            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
        }

        [Test]
        public void UnexpectedError_ShowsUnexpectedAndLogsException()
        {
            var error = new IOException("тест");
            _context.EnterLoading();

            _context.Provider.Fail(error);

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.UnexpectedError));
            Assert.That(_context.Logger.Exceptions, Is.EqualTo(new[] { error }));
        }

        [Test]
        public void FactoryThrows_ShowsUnexpectedError()
        {
            // Так ведёт себя фабрика при неверной ссылке на статью
            _context.ProviderFactory.CreateException = new ArgumentException("неверная ссылка");

            _context.EnterLoading();

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.UnexpectedError));
            Assert.That(_context.Logger.Exceptions, Has.Count.EqualTo(1));
        }

        [TestCase("")]
        [TestCase("   \n ")]
        [TestCase("☃❤")]
        public void NothingToTypeAfterNormalization_ShowsUnexpectedError(string rawText)
        {
            _context.EnterLoading();

            _context.Provider.Complete(rawText);

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.UnexpectedError));
            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<LoadingState>());
        }

        [Test]
        public void ForeignCancellation_ShowsUnexpectedError()
        {
            // Отмена, которую запросило не состояние, — не уход с экрана, а ошибка
            _context.EnterLoading();

            _context.Provider.Fail(new OperationCanceledException());

            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.UnexpectedError));
        }

        [Test]
        public void Exit_WhileLoading_CancelsRequestSilently()
        {
            _context.EnterLoading();

            _context.Machine.Exit();

            Assert.That(_context.Provider.LastToken.IsCancellationRequested, Is.True);
            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Hidden));
            Assert.That(_context.Logger.Warnings, Is.Empty);
            Assert.That(_context.Logger.Exceptions, Is.Empty);
        }

        [Test]
        public void TextArrivesAfterExit_DoesNotEnterTyping()
        {
            _context.Provider.IgnoreCancellation = true;
            _context.EnterLoading();
            _context.Machine.Exit();

            _context.Provider.Complete("текст");

            Assert.That(_context.Machine.CurrentState, Is.Null);
            Assert.That(_context.TypingScreen.BeginCount, Is.Zero);
        }

        [Test]
        public void Exit_UnsubscribesFromRetry()
        {
            _context.EnterLoading();

            _context.Machine.Exit();

            Assert.That(_context.LoadingScreen.HasRetrySubscribers, Is.False);
            Assert.That(_context.LoadingScreen.HasMenuSubscribers, Is.False);
        }

        [Test]
        public void MenuRequested_WhileLoading_CancelsAndEntersMenuSilently()
        {
            _context.EnterLoading();

            _context.LoadingScreen.ClickMenu();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<MenuState>());
            Assert.That(_context.Provider.LastToken.IsCancellationRequested, Is.True);
            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Hidden));
            Assert.That(_context.MenuScreen.IsShown, Is.True);
            Assert.That(_context.Logger.Warnings, Is.Empty);
            Assert.That(_context.Logger.Exceptions, Is.Empty);
        }

        [Test]
        public void MenuRequested_AfterError_EntersMenu()
        {
            _context.EnterLoading();
            _context.Provider.Fail(new TextLoadException(TextLoadErrorReason.Network, "тест"));

            _context.LoadingScreen.ClickMenu();

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<MenuState>());
            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Hidden));
        }

        [Test]
        public void Retry_AfterError_LoadsAgain()
        {
            _context.EnterLoading();
            _context.Provider.Fail(new TextLoadException(TextLoadErrorReason.Timeout, "тест"));

            _context.LoadingScreen.ClickRetry();

            Assert.That(_context.Provider.RequestCount, Is.EqualTo(2));
            Assert.That(_context.ProviderFactory.RequestedSources,
                Is.EqualTo(new[] { FlowTestContext.Source, FlowTestContext.Source }), "Повтор — из того же источника");
            Assert.That(_context.LoadingScreen.Current, Is.EqualTo(FakeLoadingScreen.Status.Loading));

            _context.Provider.Complete("текст");

            Assert.That(_context.Machine.CurrentState, Is.InstanceOf<TypingState>());
        }

        [Test]
        public void Constructor_NullDependency_Throws()
        {
            var machine = new GameStateMachine();
            var screen = new FakeLoadingScreen();
            var factory = new FakeTextProviderFactory();
            var options = new TextNormalizationOptions();
            var logger = new FakeAppLogger();

            Assert.Throws<ArgumentNullException>(() => new LoadingState(null, screen, factory, options, logger));
            Assert.Throws<ArgumentNullException>(() => new LoadingState(machine, null, factory, options, logger));
            Assert.Throws<ArgumentNullException>(() => new LoadingState(machine, screen, null, options, logger));
            Assert.Throws<ArgumentNullException>(() => new LoadingState(machine, screen, factory, null, logger));
            Assert.Throws<ArgumentNullException>(() => new LoadingState(machine, screen, factory, options, null));
        }
    }
}
