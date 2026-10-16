using Cysharp.Threading.Tasks;
using System.Threading;

namespace KK.App.Texts
{
    public interface ITypingTextProvider
    {
        UniTask<string> GetTextAsync(CancellationToken ct = default);
    }
}
