using Cysharp.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using UnityEngine;

public class URLArticleTextProvider : ITypingTextProvider
{
    private string _url;
    private readonly HttpClient _client;

    public URLArticleTextProvider(string url)
    {
        _url = url;
        _client = new HttpClient();
    }

    public UniTask<string> GetTextAsync(CancellationToken ct = default)
    {
        return LoadTextAsync(_url, ct);
    }

    private async UniTask<string> LoadTextAsync(string url, CancellationToken ct)
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(url, ct).AsUniTask();

            response.EnsureSuccessStatusCode();

            string htmlContent = await response.Content.ReadAsStringAsync().AsUniTask();

            KKnightsPostParser.ParsedPost parsedPost = KKnightsPostParser.Parse(htmlContent); 

            Debug.Log("Title: " + parsedPost.Title);
            Debug.Log("Text:\n" + parsedPost.Text);

            return parsedPost.Paragraphs[0];

        }
        catch (OperationCanceledException)
        {
            Debug.Log("Загрузка отменена.");
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при использовании HttpClient: {e.Message}");
            throw;
        }
    }
}