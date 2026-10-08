using Cysharp.Threading.Tasks;
using HtmlAgilityPack;
using Mono.Cecil;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

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

            PostParser.ParsedPost parsedPost = PostParser.Parse(htmlContent); 

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