using Cysharp.Threading.Tasks;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using HtmlAgilityPack;

public class URLArticleTextProvider : ITypingTextProvider
{
    private string _url;
    private readonly HttpClient _client;

    public URLArticleTextProvider(string url)
    {
        _url = url;
        _client = new HttpClient();
    }

    // UnityWebRequest не работает по TLS 1.3 (хотя на этой версии Unity должен), а КК по TLS 1.2
    // Потому заменил на HttpClient
    // TODO: Сделать вилку на UnityWebRequest и HttpClient взависимости от платформы
    // Остается только молиться, что в вебе будет работать с tls 1.3 через браузер

    public UniTask<string> GetTextAsync(CancellationToken ct = default)
    {
        return LoadTextAsync(_url, ct);
    }

    // В старых статьях нету "document-content", так что надо переделать
    // TODO: парсить не html, а json документ, котрый уже есть во всех статьях
    private async UniTask<string> LoadTextAsync(string url, CancellationToken ct)
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(url, ct).AsUniTask();

            response.EnsureSuccessStatusCode();

            string htmlContent = await response.Content.ReadAsStringAsync().AsUniTask();
            string paragraph = ExtractFirstParagraph(htmlContent);

            Debug.Log(response.StatusCode + ", " + htmlContent.Length);

            return paragraph;

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

    private string ExtractFirstParagraph(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var contentDiv = doc.DocumentNode.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' document-content ')]");

        if (contentDiv == null)
        {
            Debug.LogError("Блок с классом 'document-content' не найден.");
            throw new Exception();
        }

        var paragraphs = contentDiv.SelectNodes(".//p");

        StringBuilder resultText = new StringBuilder();

        if (paragraphs != null)
        {
            for(int i = 0; i < 1; i++)
            {
                string text = paragraphs[i].InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    resultText.AppendLine(text);
                }
            }

            Debug.Log("Извлеченный текст:\n" + resultText.ToString());
        }

        return resultText.ToString();
    }
   
}