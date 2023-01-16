using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;
// ReSharper disable ExplicitCallerInfoArgument

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static class ILog_Func
    {

        #region Func with return value only

        ///// <summary>
        ///// Call a method to get the result and intercept / log what was returned
        ///// </summary>
        ///// <returns></returns>
        //[PrivateApi]
        //public static T GetAndLog<T>(this ILog log,
        //    Func<T> func,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.Func(null, func, cPath: cPath, cName: cName, cLine: cLine);

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            Func<TResult> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, true, null, null, timer,
            Create(cPath, cName, cLine));


        /// <summary>
        /// Call a method to get the result and intercept / log what was returned
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
        public static T Func<T>(this ILog log,
            string parameters,
            Func<T> func,
            bool timer = default,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, true, parameters, message, timer, Create(cPath, cName, cLine));


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        private static TResult FuncInternal<TResult>(this ILog log,
            Func<TResult> func,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = log.FnCode<TResult>(parameters, message, timer, code);
            var result = func();
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }


        #endregion

        #region Func with Result/Message returned

        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<(TResult Result, string Message)> func,
            bool timer = default,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, true, parameters, message, timer,
            Create(cPath, cName, cLine));


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        public static TResult Func<TResult>(this ILog log,
            Func<(TResult Result, string Message)> func,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncInternal(func, false, true, null, null, timer,
            Create(cPath, cName, cLine));


        [PrivateApi]
        // 2dm - experimental, don't use yet...
        internal static TResult FuncInternal<TResult>(this ILog log,
            Func<(TResult Result, string Message)> func,
            bool isProperty,
            bool logResult,
            string parameters,
            string message,
            bool timer,
            CodeRef code
        )
        {
            var l = new LogCall<TResult>(log, code, isProperty, parameters, message, timer);
            var result = func();
            return logResult ? l.ReturnAndLog(result.Result, result.Message) : l.Return(result.Result, result.Message);
        }


        #endregion





    }
}
