using System;
using System.Runtime.CompilerServices;
using ToSic.Eav.Logging.Call;
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

        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternal(message, new CodeRef(cPath, cName, cLine));
        
        public static void A(this ILog log, 
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) { if (enabled) log?.AddInternal(message, new CodeRef(cPath, cName, cLine)); }


        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (log == null || !enabled) return null;
            return new Log(name, log);
        }

        //public static Entry AddAndReturnEntry(this ILog log, string message, CodeRef codeRef)
        //    => (log as Log)?.AddInternal(message, codeRef);

        public static LogCall<T> Call2<T>(this ILog log,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            CodeRef code = null,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall<T>(log, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);

        public static LogCall<T> Call2<T>(this ILog log,
            bool enabled,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            CodeRef code = null,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall<T>(enabled ? log : null, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);


        public static LogCall Call2(this ILog log,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall(log, new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);


        public static LogCall Call2(this ILog log, 
            Func<string> parameters, 
            Func<string> message = null,
            bool startTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => log.Call2(parameters: LogExtensionsInternal.Try(parameters),
                message: message != null ? LogExtensionsInternal.Try(message) : null,
                startTimer: startTimer,
                cPath: cPath,
                cName: cName,
                cLine: cLine
            );

        public static LogCall<T> Call2<T>(this ILog log, 
            Func<string> parameters, 
            Func<string> message = null,
            bool startTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => log.Call2<T>(parameters: LogExtensionsInternal.Try(parameters),
                message: message != null ? LogExtensionsInternal.Try(message) : null,
                startTimer: startTimer,
                cPath: cPath,
                cName: cName,
                cLine: cLine
            );
        #region Intercept

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
        public static T Return<T>(this ILog log,
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log.Return(null, generate, cPath, cName, cLine);


        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
        public static T Return<T>(this ILog log,
            string message, 
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            var callLog = log.Call2<T>(message: message, code: new CodeRef(cPath, cName, cLine));
            var result = generate();
            return callLog.ReturnAndLog(result);
        }
        #endregion
    }
}
