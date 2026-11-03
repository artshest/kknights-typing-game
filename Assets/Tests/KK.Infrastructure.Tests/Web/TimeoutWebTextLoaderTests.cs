using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Infrastructure.Tests.Fakes;
using KK.Infrastructure.Web;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace KK.Infrastructure.Tests.Web
{
    public class TimeoutWebTextLoaderTests
    {
        private const string Url = "https://example.com/";

        // Таймер CancelAfterSlim идёт в PlayerLoop, а синхронный GetResult() его бы не дождался — отсюда [UnityTest]
        [UnityTest]
        public IEnumerator LoadAsync_InnerHangs_ThrowsTimeout() => UniTask.ToCoroutine(async () =>
        {
            var loader = new TimeoutWebTextLoader(DelegateWebTextLoader.Hanging(), TimeSpan.FromMilliseconds(50));

            var e = await AsyncAssert.ThrowsAsync<TextLoadException>(() => loader.LoadAsync(Url));

            Assert.That(e.Reason, Is.EqualTo(TextLoadErrorReason.Timeout));
            Assert.That(e.InnerException, Is.InstanceOf<OperationCanceledException>());
        });

        [UnityTest]
        public IEnumerator LoadAsync_CancelledFromOutside_ThrowsOperationCanceled() => UniTask.ToCoroutine(async () =>
        {
            var loader = new TimeoutWebTextLoader(DelegateWebTextLoader.Hanging(), TimeSpan.FromSeconds(30));
            using var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromMilliseconds(50), DelayType.Realtime);

            var e = await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => loader.LoadAsync(Url, cts.Token));

            Assert.That(e, Is.Not.InstanceOf<TextLoadException>());
        });

        [Test]
        public void LoadAsync_InnerResponds_ReturnsText()
        {
            var loader = new TimeoutWebTextLoader(new FakeWebTextLoader("ответ"), TimeSpan.FromSeconds(30));

            Assert.That(loader.LoadAsync(Url).GetAwaiter().GetResult(), Is.EqualTo("ответ"));
        }

        [Test]
        public void LoadAsync_InnerThrowsTextLoadException_PassesItThrough()
        {
            var original = new TextLoadException(TextLoadErrorReason.NotFound, "404");
            var inner = new DelegateWebTextLoader((_, _) => UniTask.FromException<string>(original));
            var loader = new TimeoutWebTextLoader(inner, TimeSpan.FromSeconds(30));

            var e = Assert.Throws<TextLoadException>(() => loader.LoadAsync(Url).GetAwaiter().GetResult());

            Assert.That(e, Is.SameAs(original));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_NotPositiveTimeout_Throws(int milliseconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TimeoutWebTextLoader(new FakeWebTextLoader(""), TimeSpan.FromMilliseconds(milliseconds)));
        }

        [Test]
        public void Constructor_NullInner_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeoutWebTextLoader(null, TimeSpan.FromSeconds(1)));
        }
    }
}
