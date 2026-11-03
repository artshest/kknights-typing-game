using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Infrastructure.Tests.Fakes;
using KK.Infrastructure.Web;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace KK.Infrastructure.Tests.Web
{
    public class HttpClientTextLoaderTests
    {
        private const string Url = "https://example.com/";

        // HttpClient может продолжить работу в другом потоке, поэтому ждём через await, а не GetResult()
        [UnityTest]
        public IEnumerator LoadAsync_Success_ReturnsBody() => UniTask.ToCoroutine(async () =>
        {
            using var loader = new HttpClientTextLoader(FakeHttpMessageHandler.Status(HttpStatusCode.OK, "тело"));

            Assert.That(await loader.LoadAsync(Url), Is.EqualTo("тело"));
        });

        [UnityTest]
        public IEnumerator LoadAsync_NotFound_ThrowsNotFound() => UniTask.ToCoroutine(async () =>
        {
            using var loader = new HttpClientTextLoader(FakeHttpMessageHandler.Status(HttpStatusCode.NotFound));

            var e = await AsyncAssert.ThrowsAsync<TextLoadException>(() => loader.LoadAsync(Url));

            Assert.That(e.Reason, Is.EqualTo(TextLoadErrorReason.NotFound));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ServerError_ThrowsNetwork() => UniTask.ToCoroutine(async () =>
        {
            using var loader = new HttpClientTextLoader(FakeHttpMessageHandler.Status(HttpStatusCode.InternalServerError));

            var e = await AsyncAssert.ThrowsAsync<TextLoadException>(() => loader.LoadAsync(Url));

            Assert.That(e.Reason, Is.EqualTo(TextLoadErrorReason.Network));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ConnectionFails_ThrowsNetworkWithInner() => UniTask.ToCoroutine(async () =>
        {
            var failure = new HttpRequestException("No such host is known");
            using var loader = new HttpClientTextLoader(new FakeHttpMessageHandler(_ => throw failure));

            var e = await AsyncAssert.ThrowsAsync<TextLoadException>(() => loader.LoadAsync(Url));

            Assert.That(e.Reason, Is.EqualTo(TextLoadErrorReason.Network));
            Assert.That(e.InnerException, Is.SameAs(failure));
        });

        [UnityTest]
        public IEnumerator LoadAsync_Cancelled_ThrowsOperationCanceled() => UniTask.ToCoroutine(async () =>
        {
            using var loader = new HttpClientTextLoader(FakeHttpMessageHandler.Status(HttpStatusCode.OK));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var e = await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => loader.LoadAsync(Url, cts.Token));

            Assert.That(e, Is.Not.InstanceOf<TextLoadException>());
        });

        [Test]
        public void Constructor_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpClientTextLoader(null));
        }
    }
}
