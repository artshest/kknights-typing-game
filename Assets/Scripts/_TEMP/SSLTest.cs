using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class SSLTest : MonoBehaviour
{
    private readonly string[] testUrls = new string[]
    {
        "https://www.google.com",
        "https://api.github.com",       
        "https://kknights.com/bonfire"
    };

    void Start()
    {
        StartCoroutine(RunAllTests());
    }

    IEnumerator RunAllTests()
    {
        Debug.Log("=== ЗАПУСК ТЕСТА SSL ===");

        foreach (string url in testUrls)
        {
            yield return StartCoroutine(TestUrl(url));
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("=== ТЕСТ ЗАВЕРШЁН ===");
    }

    IEnumerator TestUrl(string url)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 15;

            float startTime = Time.realtimeSinceStartup;
            yield return req.SendWebRequest();
            float elapsed = Time.realtimeSinceStartup - startTime;

            string status = req.result switch
            {
                UnityWebRequest.Result.Success => "✅ УСПЕХ",
                UnityWebRequest.Result.ConnectionError => "❌ ОШИБКА СОЕДИНЕНИЯ",
                UnityWebRequest.Result.ProtocolError => "⚠️ ОШИБКА ПРОТОКОЛА",
                UnityWebRequest.Result.DataProcessingError => "⚠️ ОШИБКА ДАННЫХ",
                _ => "❓ НЕИЗВЕСТНО"
            };

            Debug.Log($"[{status}] {url}\n" +
                      $"  Код ответа: {req.responseCode}\n" +
                      $"  Время: {elapsed:F2} сек\n" +
                      $"  Ошибка: {(string.IsNullOrEmpty(req.error) ? "нет" : req.error)}\n" +
                      $"  Размер ответа: {req.downloadHandler?.data?.Length ?? 0} байт");
        }
    }
}