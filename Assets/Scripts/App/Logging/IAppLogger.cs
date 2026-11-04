using System;

namespace KK.App.Logging
{
    // Порт, потому что App не обращается к Debug из Unity. Имя не ILogger — оно столкнулось бы с UnityEngine.ILogger
    public interface IAppLogger
    {
        void Warning(string message);
        void Exception(Exception exception);
    }
}
