using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Various extensions for <see cref="ILog"/> objects to add logs.
    /// They are all implemented as extension methods, so that they will not fail even if the log object is null.
    /// </summary>
    [PublicApi]
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {
        [PrivateApi]
        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (!enabled) return null;
            log = log.GetRealLog();
            return log == default ? null : new Log(name, log);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void Wrp(this ILog log,
            Func<ILogCall, string> action,
            string parameters = default,
            string message = default,
            bool timer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log.GetRealLog(), UseOrCreate(code, cPath, cName, cLine), false, parameters, message, timer);
            var finalMsg = action(l);
            l.Done(finalMsg);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void Do(this ILog log,
            Action action,
            string parameters = default,
            string message = default,
            bool timer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log.GetRealLog(), UseOrCreate(code, cPath, cName, cLine), false, parameters, message, timer);
            action();
            l.Done();
        }


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static void DoAndLog(this ILog log,
            Func<string> action,
            string parameters = default,
            string message = default,
            bool timer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log.GetRealLog(), UseOrCreate(code, cPath, cName, cLine), false, parameters, message, timer);
            var finalMsg = action();
            l.Done(finalMsg);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult WrpFn<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> action,
            string parameters = default,
            string message = default,
            bool timer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueAndMessage(action, false, false, parameters, message, timer,
            UseOrCreate(code, cPath, cName, cLine));

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult WrpFn<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> action,
            string parameters = default,
            string message = default,
            bool timer = default,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueOnly(action, false, false, parameters, message, timer,
            UseOrCreate(code, cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which only return the value.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Get<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueOnly(action, true, false, null, null, timer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value and a message.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Get<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueAndMessage(action, true, false, null, null, timer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value, and log the result.
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        public static TResult GetAndLog<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueOnly(action, true, true, null, null, timer, Create(cPath, cName, cLine));

        /// <summary>
        /// Short wrapper for Get-property calls which return the value, and log the result and a message
        /// </summary>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <returns></returns>
        [PrivateApi]
        public static TResult GetAndLog<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.WrapValueAndMessage(action, true, true, null, null, timer, Create(cPath, cName, cLine));



        [PrivateApi]
        // 2dm - experimental, don't use yet...
        private static TResult WrapValueOnly<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> action,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log.GetRealLog(), code, isProperty, parameters, message, timer);
            var result = action(l);
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        private static TResult WrapValueAndMessage<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> action,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log.GetRealLog(), code, isProperty, parameters, message, timer);
            var result = action(l);
            return logResult ? l.ReturnAndLog(result.Result, result.Message) : l.Return(result.Result, result.Message);
        }
    }
}
