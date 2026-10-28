using Cysharp.Threading.Tasks;
using System;
using System.Net.Http;
using System.Threading;

namespace KK.Infrastructure.Web
{
    // В WebGL не работает: HttpClient нужны сокеты, а браузер их не даёт
    public class HttpClientTextLoader : IWebTextLoader, IDisposable
    {
        private readonly HttpClient _client = new();

        public async UniTask<string> LoadAsync(string url, CancellationToken ct = default)
        {
            using HttpResponseMessage response = await _client.GetAsync(url, ct).AsUniTask();
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().AsUniTask();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
