using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {
        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall<T>(log, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log,
            bool enabled,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall<T>(enabled ? log : null, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);


        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall Fn(this ILog log,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall(log, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);



        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall Fn(this ILog log,
            Func<string> parameters,
            Func<string> message = default,
            bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.Fn(parameters: LogExtensionsInternal.Try(parameters),
            message: message != default ? LogExtensionsInternal.Try(message) : null,
            startTimer: startTimer,
            // ReSharper disable ExplicitCallerInfoArgument
            cPath: cPath,
            cName: cName,
            cLine: cLine
            // ReSharper restore ExplicitCallerInfoArgument
        );

        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
        public static LogCall<T> Fn<T>(this ILog log,
            Func<string> parameters,
            Func<string> message = default,
            bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.Fn<T>(parameters: LogExtensionsInternal.Try(parameters),
            message: message != default ? LogExtensionsInternal.Try(message) : null,
            startTimer: startTimer,
            // ReSharper disable ExplicitCallerInfoArgument
            cPath: cPath,
            cName: cName,
            cLine: cLine
            // ReSharper restore ExplicitCallerInfoArgument
        );



    }
}
