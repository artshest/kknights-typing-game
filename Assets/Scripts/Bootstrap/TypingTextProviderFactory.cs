using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class TypingTextProviderFactory : MonoBehaviour
{
    public enum ProviderType
    {
        PredefinedText,
        PredefinedArticleURL
    }

    [SerializeField]
    private ProviderType _type;

    [SerializeField]
    [TextArea(5, 10)]
    private string _predefinedText;
    [SerializeField]
    private string _articleUrlApi;

    private ITypingTextProvider _chachedProvider;

    public ITypingTextProvider Create()
    {
        if (_chachedProvider != null)
        {
            return _chachedProvider;
        }

        _chachedProvider = _type switch
        {
            ProviderType.PredefinedText => new PredefinedTextProvider(_predefinedText),
            ProviderType.PredefinedArticleURL => new URLArticleTextProvider(_articleUrlApi),
            _ => throw new ArgumentOutOfRangeException(nameof(_type))
        };

        return _chachedProvider;
    }

    private void OnDisable()
    {
        if (_chachedProvider is IDisposable d) d.Dispose();
        _chachedProvider = null;
    }
}

