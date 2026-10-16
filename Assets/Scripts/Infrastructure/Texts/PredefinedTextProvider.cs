using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class PredefinedTextProvider : ITypingTextProvider
{
    private readonly string _text;

    public PredefinedTextProvider(string text)
    {
        _text = text;
    }

    public UniTask<string> GetTextAsync(CancellationToken ct = default)
    {
        return new UniTask<string>(_text);
    }
}