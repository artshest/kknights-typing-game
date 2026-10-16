using System.Collections;
using UnityEngine;

namespace KK.Infrastructure.Coroutines
{
    // Не очень мне нравится вся эта статика
    // Если внедрять DI фреймворк, возможно стоит это все переделать

    public static class CoroutineService
    {
        private static CoroutineHost _coroutineHost;

        private static CoroutineHost Host
        {
            get
            {
                if (_coroutineHost == null)
                {
                    var go = new GameObject("CoroutineRunner");
                    _coroutineHost = go.AddComponent<CoroutineHost>();
                    GameObject.DontDestroyOnLoad(go);
                }
                return _coroutineHost;
            }
            set
            {
                _coroutineHost = value;
            }
        }

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return Host.StartCoroutine(routine);
        }
    }

    public class CoroutineHost : MonoBehaviour
    {
    }
}
