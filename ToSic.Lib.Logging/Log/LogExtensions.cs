using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {
        /// <summary>
        /// Attach this log to another parent log, which should also know about events logged here.
        /// </summary>
        /// <param name="hasLog">thing which has a log</param>
        /// <param name="parentLog">The parent log</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void LinkLog(this IHasLog hasLog, ILog parentLog) => (hasLog.Log as Log)?.LinkTo(parentLog);

        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternal(message, new CodeRef(cPath, cName, cLine));


        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternal(LogExtensionsInternal.Try(messageMaker), new CodeRef(cPath, cName, cLine));

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static string AddAndReuse(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternalReuse(message, new CodeRef(cPath, cName, cLine)).Message;


        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) { if (enabled) log?.AddInternal(message, new CodeRef(cPath, cName, cLine)); }

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void W(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
            => log.A("WARNING: " + message, cPath, cName, cLine);


        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (log == null || !enabled) return null;
            return new Log(name, log);
        }

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            CodeRef code = null,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall<T>(log, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log,
            bool enabled,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            CodeRef code = null,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall<T>(enabled ? log : null, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);


        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall Fn(this ILog log,
            string parameters = null,
            string message = null,
            bool startTimer = false,
            CodeRef code = null,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => new LogCall(log, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall Fn(this ILog log, 
            Func<string> parameters, 
            Func<string> message = null,
            bool startTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => log.Fn(parameters: LogExtensionsInternal.Try(parameters),
                message: message != null ? LogExtensionsInternal.Try(message) : null,
                startTimer: startTimer,
                cPath: cPath,
                cName: cName,
                cLine: cLine
            );

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log, 
            Func<string> parameters, 
            Func<string> message = null,
            bool startTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => log.Fn<T>(parameters: LogExtensionsInternal.Try(parameters),
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
// TODO: This should be renamed to DoAndReturn() for clarity
// TODO: Not Null Safe! must fix
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
        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
// TODO: This should be renamed to DoAndReturn() for clarity
        public static T Return<T>(this ILog log,
            string message, 
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            var callLog = log.Fn<T>(message: message, code: new CodeRef(cPath, cName, cLine));
            var result = generate();
            return callLog.ReturnAndLog(result);
        }
        #endregion
    }
}
