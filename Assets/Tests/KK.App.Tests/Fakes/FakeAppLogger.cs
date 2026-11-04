using System;
using System.Collections.Generic;
using KK.App.Logging;

namespace KK.App.Tests.Fakes
{
    public class FakeAppLogger : IAppLogger
    {
        public List<string> Warnings { get; } = new();
        public List<Exception> Exceptions { get; } = new();

        public void Warning(string message)
        {
            Warnings.Add(message);
        }

        public void Exception(Exception exception)
        {
            Exceptions.Add(exception);
        }
    }
}
