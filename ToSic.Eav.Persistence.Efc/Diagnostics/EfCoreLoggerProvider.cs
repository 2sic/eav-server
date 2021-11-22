using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ToSic.Eav.Persistence.Efc.Diagnostics
{
    // this class helps debug in advanced scenarios
    // hasn't been used since ca. 2017, but keep in case we ever do deep work on the DB again
    // ReSharper disable once UnusedMember.Global
#if NET472
    public class EfCoreLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }

        public void Dispose()
        { }

        private class MyLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                File.AppendAllText(@"C:\temp\log.txt", formatter(state, exception));
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
#endif
}
