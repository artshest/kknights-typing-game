using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public interface ITypingTextProvider
{
    UniTask<string> GetTextAsync(CancellationToken ct = default);
}