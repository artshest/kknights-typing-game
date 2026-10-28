using Cysharp.Threading.Tasks;
using KK.App.Texts;
using System;
using System.Collections.Generic;
using System.Threading;

namespace KK.Infrastructure.Texts
{
    public class RandomTextProvider : ITypingTextProvider
    {
        private readonly IReadOnlyList<string> _texts;
        private readonly Random _random;
        private int _lastIndex = -1;

        public RandomTextProvider(IReadOnlyList<string> texts, Random random)
        {
            if (texts == null) throw new ArgumentNullException(nameof(texts));
            if (texts.Count == 0) throw new ArgumentException("Список текстов пуст", nameof(texts));

            _texts = texts;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public UniTask<string> GetTextAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            _lastIndex = NextIndex();
            return UniTask.FromResult(_texts[_lastIndex]);
        }

        private int NextIndex()
        {
            if (_lastIndex < 0)
                return _random.Next(_texts.Count);

            // Сдвиг на 1..Count-1 от прошлого индекса: повтор подряд невозможен, остальные тексты равновероятны.
            // При одном тексте Next(1, 1) вернёт 0 — тот же текст, другого просто нет
            return (_lastIndex + _random.Next(1, _texts.Count)) % _texts.Count;
        }
    }
}
