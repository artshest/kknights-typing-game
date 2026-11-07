using KK.App.Texts;
using System;

namespace KK.Infrastructure.Texts
{
    // Отдаёт тот же провайдер, пока источник не сменился. RandomTextProvider помнит прошлый текст: новый
    // экземпляр на каждый «Другой текст» сломал бы правило «без повтора подряд».
    // Как именно создать провайдер, решает тот, кто передал createProvider (настройки из инспектора — в Bootstrap)
    public sealed class CachedTextProviderFactory : ITypingTextProviderFactory, IDisposable
    {
        private readonly Func<TextSource, ITypingTextProvider> _createProvider;

        private TextSource _cachedSource;
        private ITypingTextProvider _cachedProvider;

        public CachedTextProviderFactory(Func<TextSource, ITypingTextProvider> createProvider)
        {
            _createProvider = createProvider ?? throw new ArgumentNullException(nameof(createProvider));
        }

        public ITypingTextProvider Create(TextSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (_cachedProvider != null && source.Equals(_cachedSource))
                return _cachedProvider;

            // Сначала создаём новый: если создание бросит, прежний провайдер останется рабочим
            ITypingTextProvider provider = _createProvider(source)
                ?? throw new InvalidOperationException($"Не создан провайдер для источника {source.Kind}");

            DisposeCached();
            _cachedSource = source;
            _cachedProvider = provider;
            return provider;
        }

        public void Dispose()
        {
            DisposeCached();
        }

        private void DisposeCached()
        {
            if (_cachedProvider is IDisposable disposable)
                disposable.Dispose();

            _cachedProvider = null;
            _cachedSource = null;
        }
    }
}
