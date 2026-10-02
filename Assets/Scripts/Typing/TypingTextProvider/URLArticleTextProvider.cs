using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class URLArticleTextProvider : ITypingTextProvider
{
    //TODO: написать парсер статьи КК
    //<div class="document-content"

    public UniTask<string> GetTextAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
