using Cysharp.Threading.Tasks;
using KK.App.Texts;
using System;
using System.Threading;

namespace KK.Infrastructure.Web
{
    // Декоратор: таймаут написан один раз для любого загрузчика, сами загрузчики о нём не знают
    public class TimeoutWebTextLoader : IWebTextLoader
    {
        private readonly IWebTextLoader _inner;
        private readonly TimeSpan _timeout;

        public TimeoutWebTextLoader(IWebTextLoader inner, TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Таймаут должен быть больше нуля");

            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _timeout = timeout;
        }

        public async UniTask<string> LoadAsync(string url, CancellationToken ct = default)
        {
            using var timeoutSource = new CancellationTokenSource();
            // CancelAfter работает через поток таймера, которого нет в WebGL; Slim-версия считает время в PlayerLoop.
            // Realtime, а не DeltaTime: при Time.timeScale = 0 (пауза) таймаут иначе никогда бы не сработал
            using IDisposable timer = timeoutSource.CancelAfterSlim(_timeout, DelayType.Realtime);
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutSource.Token);

            try
            {
                return await _inner.LoadAsync(url, linkedSource.Token);
            }
            // Отмену снаружи (уничтожили объект) пробрасываем как есть — это не ошибка загрузки
            catch (OperationCanceledException e) when (timeoutSource.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                throw new TextLoadException(TextLoadErrorReason.Timeout,
                    $"Нет ответа от {url} за {_timeout.TotalSeconds} с", e);
            }
        }
    }
}
