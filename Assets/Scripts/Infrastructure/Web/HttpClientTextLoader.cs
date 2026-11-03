using Cysharp.Threading.Tasks;
using KK.App.Texts;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace KK.Infrastructure.Web
{
    // В WebGL не работает: HttpClient нужны сокеты, а браузер их не даёт
    public class HttpClientTextLoader : IWebTextLoader, IDisposable
    {
        private readonly HttpClient _client;

        public HttpClientTextLoader() : this(new HttpClientHandler())
        {
        }

        // Обработчик снаружи — чтобы тесты подменяли сеть
        public HttpClientTextLoader(HttpMessageHandler handler)
        {
            _client = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler)))
            {
                // Свой таймаут (100 с) отключён: его срабатывание — TaskCanceledException, неотличимое от отмены.
                // Таймаут задаёт TimeoutWebTextLoader
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public async UniTask<string> LoadAsync(string url, CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(url, ct).AsUniTask();

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new TextLoadException(TextLoadErrorReason.NotFound, $"404 по адресу {url}");
                if (!response.IsSuccessStatusCode)
                    throw new TextLoadException(TextLoadErrorReason.Network,
                        $"{(int)response.StatusCode} {response.ReasonPhrase} по адресу {url}");

                return await response.Content.ReadAsStringAsync().AsUniTask();
            }
            // IOException — обрыв соединения посреди чтения ответа
            catch (Exception e) when (e is HttpRequestException or IOException)
            {
                throw new TextLoadException(TextLoadErrorReason.Network, $"Не удалось загрузить {url}", e);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
