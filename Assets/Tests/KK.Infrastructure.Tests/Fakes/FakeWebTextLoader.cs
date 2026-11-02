using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.Infrastructure.Web;

namespace KK.Infrastructure.Tests.Fakes
{
    public class FakeWebTextLoader : IWebTextLoader
    {
        private readonly string _response;

        public FakeWebTextLoader(string response)
        {
            _response = response;
        }

        public List<string> RequestedUrls { get; } = new();

        public UniTask<string> LoadAsync(string url, CancellationToken ct = default)
        {
            RequestedUrls.Add(url);
            return UniTask.FromResult(_response);
        }
    }
}
