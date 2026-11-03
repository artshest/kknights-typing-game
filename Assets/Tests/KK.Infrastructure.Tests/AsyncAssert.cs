using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace KK.Infrastructure.Tests
{
    // Assert.ThrowsAsync из NUnit ждёт Task и блокирует поток, а продолжения UniTask идут через PlayerLoop
    // того же потока — поэтому ждём через await внутри [UnityTest]
    public static class AsyncAssert
    {
        public static async UniTask<T> ThrowsAsync<T>(Func<UniTask> action) where T : Exception
        {
            try
            {
                await action();
            }
            catch (T e)
            {
                return e;
            }

            Assert.Fail($"Ожидалось исключение {typeof(T).Name}");
            return null;
        }
    }
}
