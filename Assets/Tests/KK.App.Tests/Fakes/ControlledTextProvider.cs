using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KK.App.Texts;

namespace KK.App.Tests.Fakes
{
    // Тест сам решает, когда и чем закончится загрузка. Продолжение UniTask выполняется синхронно в момент
    // Complete/Fail, поэтому тестам не нужен ни PlayerLoop, ни [UnityTest]
    public class ControlledTextProvider : ITypingTextProvider
    {
        private UniTaskCompletionSource<string> _completion;

        public int RequestCount { get; private set; }
        public CancellationToken LastToken { get; private set; }
        // Провайдер, который не слушает токен: текст приходит, даже если загрузку уже отменили
        public bool IgnoreCancellation { get; set; }

        public UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            RequestCount++;
            LastToken = ct;
            _completion = new UniTaskCompletionSource<string>();

            if (!IgnoreCancellation)
            {
                UniTaskCompletionSource<string> completion = _completion;
                ct.Register(() => completion.TrySetCanceled(ct));
            }

            return _completion.Task;
        }

        public void Complete(string text)
        {
            _completion.TrySetResult(text);
        }

        public void Fail(Exception exception)
        {
            _completion.TrySetException(exception);
        }
    }

    public class FakeTextProviderFactory : ITypingTextProviderFactory
    {
        public ControlledTextProvider Provider { get; } = new();
        public Exception CreateException { get; set; }
        public List<TextSource> RequestedSources { get; } = new();

        public ITypingTextProvider Create(TextSource source)
        {
            RequestedSources.Add(source);

            if (CreateException != null)
                throw CreateException;

            return Provider;
        }
    }
}
