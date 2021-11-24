﻿using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ToSic.Eav.Persistence.Efc.Diagnostics
{
    // this class helps debug in advanced scenarios
    // hasn't been used since ca. 2017, but keep in case we ever do deep work on the DB again
    // ReSharper disable once UnusedMember.Global
#if NETFRAMEWORK
    public class EfCoreFilteredLoggerProvider : ILoggerProvider
    {
        private static string[] _categories =
        {
            typeof(Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommandBuilderFactory).FullName,
//#if NETFRAMEWORK
//            typeof(Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection).FullName
//#endif
        };

        public ILogger CreateLogger(string categoryName)
        {
            if (_categories.Contains(categoryName))
            {
                return new MyLogger();
            }

            return new NullLogger();
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
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }

        private class NullLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            { }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
#endif
}
