using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.App.Texts;
using KK.Infrastructure.Texts;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts
{
    public class CachedTextProviderFactoryTests
    {
        private const string Link = "https://kknights.com/posts/test";

        private List<DisposableProvider> _created;
        private Exception _createException;
        private CachedTextProviderFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _created = new List<DisposableProvider>();
            _createException = null;
            _factory = new CachedTextProviderFactory(source =>
            {
                if (_createException != null)
                    throw _createException;

                var provider = new DisposableProvider(source);
                _created.Add(provider);
                return provider;
            });
        }

        [Test]
        public void EqualSource_ReturnsSameProvider()
        {
            // Разные экземпляры, но равные источники: «Другой текст» не должен пересоздавать библиотеку
            ITypingTextProvider first = _factory.Create(TextSource.Library());
            ITypingTextProvider second = _factory.Create(TextSource.Library());

            Assert.That(second, Is.SameAs(first));
            Assert.That(_created, Has.Count.EqualTo(1));
        }

        [Test]
        public void OtherSource_CreatesNewAndDisposesOld()
        {
            var library = (DisposableProvider)_factory.Create(TextSource.Library());

            var article = (DisposableProvider)_factory.Create(TextSource.Article(Link));

            Assert.That(article, Is.Not.SameAs(library));
            Assert.That(article.Source, Is.EqualTo(TextSource.Article(Link)));
            Assert.That(library.IsDisposed, Is.True);
            Assert.That(article.IsDisposed, Is.False);
        }

        [Test]
        public void CreateThrows_KeepsPreviousProvider()
        {
            var library = (DisposableProvider)_factory.Create(TextSource.Library());
            _createException = new ArgumentException("неверная ссылка");

            Assert.Throws<ArgumentException>(() => _factory.Create(TextSource.Article(Link)));

            _createException = null;
            Assert.That(library.IsDisposed, Is.False);
            Assert.That(_factory.Create(TextSource.Library()), Is.SameAs(library));
        }

        [Test]
        public void Dispose_DisposesCachedAndNextCreateMakesNew()
        {
            var first = (DisposableProvider)_factory.Create(TextSource.Library());

            _factory.Dispose();
            ITypingTextProvider second = _factory.Create(TextSource.Library());

            Assert.That(first.IsDisposed, Is.True);
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void CreateReturnsNull_Throws()
        {
            var factory = new CachedTextProviderFactory(_ => null);

            Assert.Throws<InvalidOperationException>(() => factory.Create(TextSource.Library()));
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new CachedTextProviderFactory(null));
            Assert.Throws<ArgumentNullException>(() => _factory.Create(null));
        }

        private class DisposableProvider : ITypingTextProvider, IDisposable
        {
            public TextSource Source { get; }
            public bool IsDisposed { get; private set; }

            public DisposableProvider(TextSource source)
            {
                Source = source;
            }

            public UniTask<string> GetTextAsync(CancellationToken ct = default)
            {
                return UniTask.FromResult("текст");
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
