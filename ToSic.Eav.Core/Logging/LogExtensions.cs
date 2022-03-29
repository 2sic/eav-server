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
        
        public static void SafeAdd(this ILog log, bool enabled, string message,
                [CallerFilePath] string cPath = null,
                [CallerMemberName] string cName = null,
                [CallerLineNumber] int cLine = 0
        )
        {
            if (enabled) log?.Add(message, cPath, cName, cLine);
        }

        /// <summary>
        /// Creates a safe wrap-log function which works if the log exists or not
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a safe wrap-log function which works if the log exists or not
        /// </summary>
        /// <returns></returns>
        public static Func<string, T, T> SafeCall<T>(this ILog log,
            bool enabled,
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
            )
        {
            return enabled && log != null 
                ? log.Call<T>(parameters, message, useTimer, cPath, cName, cLine) 
                : (msg, result) => result;
        }

        /// <summary>
        /// Creates a safe wrap-log action which works if the log exists or not
        /// </summary>
        /// <returns></returns>
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


        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (log == null || !enabled) return null;
            return new Log(name, log);
        }
    }
}
