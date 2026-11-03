using System;

namespace KK.App.Texts
{
    public enum TextLoadErrorReason
    {
        Network,
        NotFound,
        Timeout,
        BadFormat
    }

    public class TextLoadException : Exception
    {
        public TextLoadErrorReason Reason { get; }

        public TextLoadException(TextLoadErrorReason reason, string message, Exception innerException = null) : base(message, innerException)
        {
            Reason = reason;
        }
    }
}
