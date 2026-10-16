using Cysharp.Threading.Tasks;
using KK.App.Texts;
using System.Threading;

namespace KK.Infrastructure.Texts
{
    public class PredefinedTextProvider : ITypingTextProvider
    {
        private readonly string _text;

        public PredefinedTextProvider(string text)
        {
            _text = text;
        }

        public UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            return new UniTask<string>(_text);
        }
    }
}
