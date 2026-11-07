using KK.App.Flow;
using KK.App.Tests.Fakes;
using KK.App.Texts;
using KK.Core.Texts;

namespace KK.App.Tests.Flow
{
    // Настоящая машина с настоящими состояниями на фейковых экранах: так проверяются и сами переходы
    public class FlowTestContext
    {
        public static readonly TextSource Source = TextSource.Article("https://kknights.com/posts/test");

        public const string DefaultLink = "https://kknights.com/posts/default";

        public readonly GameStateMachine Machine = new();
        public readonly FakeMenuScreen MenuScreen = new();
        public readonly FakeLoadingScreen LoadingScreen = new();
        public readonly FakeTypingScreen TypingScreen = new();
        public readonly FakeResultsScreen ResultsScreen = new();
        public readonly FakeTextProviderFactory ProviderFactory = new();
        public readonly FakeAppLogger Logger = new();
        public readonly FakeTypingInputFactory InputFactory = new();

        public ControlledTextProvider Provider => ProviderFactory.Provider;

        public FlowTestContext(bool replaceYo = true)
        {
            Machine.Register(new MenuState(Machine, MenuScreen, new FakeArticleLinkValidator(), DefaultLink));
            Machine.Register(new LoadingState(Machine, LoadingScreen, ProviderFactory,
                new TextNormalizationOptions { ReplaceYo = replaceYo }, Logger));
            Machine.Register(new TypingState(Machine, TypingScreen, InputFactory));
            Machine.Register(new ResultsState(Machine, ResultsScreen));
        }

        public void EnterLoading()
        {
            Machine.Enter<LoadingState, TextSource>(Source);
        }

        public void EnterTyping(string text)
        {
            Machine.Enter<TypingState, LoadedText>(new LoadedText(text, Source));
        }
    }
}
