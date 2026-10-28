using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;

namespace KK.Infrastructure.Web
{
    // В редакторе и Windows-сборке не подходит для kknights.com: сайт принимает только TLS 1.3,
    // а собственный TLS Unity (6000.6.4f1) его не поддерживает — "Unable to complete SSL connection".
    // В WebGL рукопожатие делает браузер, поэтому там ограничения нет
    public class UnityWebRequestTextLoader : IWebTextLoader
    {
        public async UniTask<string> LoadAsync(string url, CancellationToken ct = default)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            // При ошибке сети или HTTP-коде не 2xx UniTask бросает UnityWebRequestException,
            // при отмене — вызывает Abort() у запроса
            await request.SendWebRequest().WithCancellation(ct);
            return request.downloadHandler.text;
        }
    }
}
