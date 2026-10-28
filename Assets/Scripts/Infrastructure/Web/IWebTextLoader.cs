using Cysharp.Threading.Tasks;
using System.Threading;

namespace KK.Infrastructure.Web
{
    public interface IWebTextLoader
    {
        UniTask<string> LoadAsync(string url, CancellationToken ct = default);
    }
}
