using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.Infrastructure.Web;

namespace KK.Infrastructure.Tests.Fakes
{
    // Поведение задаётся лямбдой: зависнуть, бросить исключение, ответить — без отдельного класса на каждый случай
    public class DelegateWebTextLoader : IWebTextLoader
    {
        private readonly Func<string, CancellationToken, UniTask<string>> _load;

        public DelegateWebTextLoader(Func<string, CancellationToken, UniTask<string>> load)
        {
            _load = load;
        }

        public static DelegateWebTextLoader Hanging() => new((_, ct) => UniTask.Never<string>(ct));

        public UniTask<string> LoadAsync(string url, CancellationToken ct = default) => _load(url, ct);
    }
}
