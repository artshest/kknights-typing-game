using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KK.Infrastructure.Tests.Fakes
{
    // Подменяет сеть для HttpClient: запрос до сокетов не доходит
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public static FakeHttpMessageHandler Status(HttpStatusCode code, string body = "") =>
            new(_ => new HttpResponseMessage(code) { Content = new StringContent(body) });

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_respond(request));
        }
    }
}
