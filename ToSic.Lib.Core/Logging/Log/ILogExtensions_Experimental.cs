using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, false, null, null, timer,
            Create(cPath, cName, cLine));
        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<ILogCall<TResult>, (TResult Result, string Message)> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, false, parameters, null, timer,
            Create(cPath, cName, cLine));



        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, false, null, null, timer,
            Create(cPath, cName, cLine));

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<ILogCall<TResult>, TResult> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, false, parameters, null, timer,
            Create(cPath, cName, cLine));

        ///// <summary>
        ///// Short wrapper for Get-property calls which return the value, and log the result.
        ///// </summary>
        ///// <typeparam name="TResult">Type of return value</typeparam>
        ///// <returns></returns>
        //[PrivateApi]
        //public static TResult GetAndLog<TResult>(this ILog log,
        //    Func<ILogCall<TResult>, TResult> action,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.WrapValueOnly(action, true, true, null, null, timer, Create(cPath, cName, cLine));

        ///// <summary>
        ///// Short wrapper for Get-property calls which return the value, and log the result and a message
        ///// </summary>
        ///// <typeparam name="TResult">Type of return value</typeparam>
        ///// <returns></returns>
        //[PrivateApi]
        //public static TResult GetAndLog<TResult>(this ILog log,
        //    Func<ILogCall<TResult>, (TResult Result, string Message)> action,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.WrapValueAndMessage(action, true, true, null, null, timer, Create(cPath, cName, cLine));



        [PrivateApi]
        // 2dm - experimental, don't use yet...
        internal static TResult FuncInternal<TResult>(this ILog log,
            Func<ILogCall<TResult>, TResult> func,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log, code, isProperty, parameters, message, timer);
            var result = func(l);
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        internal static TResult FuncInternal<TResult>(this ILog log,
            Func<ILogCall<TResult>, (TResult Result, string Message)> func,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log, code, isProperty, parameters, message, timer);
            var (result, resultMessage) = func(l);
            return logResult ? l.ReturnAndLog(result, resultMessage) : l.Return(result, resultMessage);
        }
    }
}
