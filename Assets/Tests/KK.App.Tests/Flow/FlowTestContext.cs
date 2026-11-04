using KK.App.Flow;
using KK.App.Tests.Fakes;
using KK.Core.Texts;

namespace KK.App.Tests.Flow
{
    // Настоящая машина с настоящими состояниями на фейковых экранах: так проверяются и сами переходы
    public class FlowTestContext
    {
        public readonly GameStateMachine Machine = new();
        public readonly FakeLoadingScreen LoadingScreen = new();
        public readonly FakeTypingScreen TypingScreen = new();
        public readonly FakeResultsScreen ResultsScreen = new();
        public readonly FakeTextProviderFactory ProviderFactory = new();
        public readonly FakeAppLogger Logger = new();

        public ControlledTextProvider Provider => ProviderFactory.Provider;

        public FlowTestContext(bool replaceYo = true)
        {
            Machine.Register(new LoadingState(Machine, LoadingScreen, ProviderFactory,
                new TextNormalizationOptions { ReplaceYo = replaceYo }, Logger));
            Machine.Register(new TypingState(Machine, TypingScreen));
            Machine.Register(new ResultsState(Machine, ResultsScreen));
        }
    }
}
