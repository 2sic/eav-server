using System;
using System.Runtime.CompilerServices;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{

    public static class LogExtensions
    {
        /// <summary>
        /// Attach this log to another parent log, which should also know about events logged here.
        /// </summary>
        /// <param name="hasLog">thing which has a log</param>
        /// <param name="parentLog">The parent log</param>
        public static void LinkLog(this IHasLog hasLog, ILog parentLog) => hasLog.Log.LinkTo(parentLog);

        public static void SafeAdd(this ILog log, string message,
                [CallerFilePath] string cPath = null,
                [CallerMemberName] string cName = null,
                [CallerLineNumber] int cLine = 0
        ) => log?.Add(message, cPath, cName, cLine);
        
        public static Func<string, T, T> SafeCall<T>(this ILog log,
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
            )
        {
            return log != null 
                ? log.Call<T>(parameters, message, useTimer, cPath, cName, cLine) 
                : (msg, result) => result;
        }

        public static Action<string> SafeCall(this ILog log,
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            return log != null
                ? log.Call(parameters, message, useTimer, cPath, cName, cLine)
                : (msg) => { } ;
        }


        public static ILog SubLogOrNull(this ILog log, string name)
        {
            if (log == null) return null;
            return new Log(name, log);
        }
    }
}
