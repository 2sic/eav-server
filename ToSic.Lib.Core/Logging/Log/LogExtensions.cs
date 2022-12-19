using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {
        [PrivateApi]
        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (log?._RealLog == default || !enabled) return null;
            return new Log(name, log._RealLog);
        }


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void Wrp(this ILog log,
            Func<LogCall, string> action,
            string parameters = default,
            string message = default,
            bool startTimer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log?._RealLog, UseOrCreate(code, cPath, cName, cLine), false, parameters, message, startTimer);
            var finalMsg = action(l);
            l.Done(finalMsg);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void Do(this ILog log,
            Action action,
            string parameters = default,
            string message = default,
            bool startTimer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log?._RealLog, UseOrCreate(code, cPath, cName, cLine), false, parameters, message, startTimer);
            action();
            l.Done();
        }


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void DoAndLog(this ILog log,
            Func<string> action,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log?._RealLog, UseOrCreate(code, cPath, cName, cLine), false, parameters, message, startTimer);
            var finalMsg = action();
            l.Done(finalMsg);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult WrpFn<TResult>(this ILog log,
            Func<LogCall<TResult>, (TResult Result, string Message)> action,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            return log.WrapValueAndMessage(action, false, false, parameters, message, startTimer,
                UseOrCreate(code, cPath, cName, cLine));
            //var l = new LogCall<TResult>(log?._RealLog, UseOrCreate(code, cPath, cName, cLine), false, parameters, message, startTimer);
            //var result = action(l);
            //return l.Return(result.Result, result.Message);
        }

        /// <summary>
        /// Short wrapper for Get-property calls which only return the value.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Get<TResult>(this ILog log,
            Func<LogCall<TResult>, TResult> action,
            bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueOnly(action, true, false, null, null, startTimer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value and a message.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Get<TResult>(this ILog log,
            Func<LogCall<TResult>, (TResult Result, string Message)> action,
            bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueAndMessage(action, true, false, null, null, startTimer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value, and log the result.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        public static TResult GetAndLog<TResult>(this ILog log,
            Func<LogCall<TResult>, TResult> action,
            bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueOnly(action, true, true, null, null, startTimer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value, and log the result and a message
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        public static TResult GetAndLog<TResult>(this ILog log,
            Func<LogCall<TResult>, (TResult Result, string Message)> action, bool startTimer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueAndMessage(action, true, true, null, null, startTimer, Create(cPath, cName, cLine));



        [PrivateApi]
        // 2dm - experimental, don't use yet...
        private static TResult WrapValueOnly<TResult>(this ILog log,
            Func<LogCall<TResult>, TResult> action,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool startTimer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log?._RealLog, code, isProperty, parameters, message, startTimer);
            var result = action(l);
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        private static TResult WrapValueAndMessage<TResult>(this ILog log,
            Func<LogCall<TResult>, (TResult Result, string Message)> action,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool startTimer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log?._RealLog, code, isProperty, parameters, message, startTimer);
            var result = action(l);
            return logResult ? l.ReturnAndLog(result.Result, result.Message) : l.Return(result.Result, result.Message);
        }
    }
}
