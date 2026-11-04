using KK.App.Logging;
using System;
using UnityEngine;

namespace KK.Infrastructure.Logging
{
    public class UnityAppLogger : IAppLogger
    {
        public void Warning(string message)
        {
            Debug.LogWarning(message);
        }

        public void Exception(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
