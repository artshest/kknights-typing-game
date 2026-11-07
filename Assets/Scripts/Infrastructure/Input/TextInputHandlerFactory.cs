using KK.App.Typing;

namespace KK.Infrastructure.Input
{
    public class TextInputHandlerFactory : ITypingInputFactory
    {
        public ITypingInput Create()
        {
            return new TextInputHandler();
        }
    }
}
