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

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            Func<TResult> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncResult(func, null, message, timer, Create(cPath, cName, cLine), enabled);


        //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        //public static TResult Func<TResult>(this ILog log,
        //    Func<TResult> func,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.FuncResult(func, null, null, timer, Create(cPath, cName, cLine), enabled);


        /// <summary>
        /// Call a method to get the result and intercept / log what was returned
        /// </summary>
        /// <returns></returns>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<TResult> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncResult(func, parameters, message, timer, Create(cPath, cName, cLine), enabled);


        private static TResult FuncResult<TResult>(this ILog log,
            Func<TResult> func,
            string parameters,
            string message,
            bool timer,
            CodeRef code,
            bool enabled,
            bool logResult = true)
        {
            var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
            var result = func();
            if (!enabled) return result;
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }


        #endregion

        #region Func with Result/Message returned

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            Func<(TResult Result, string FinalMessage)> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncMessage(func, null, message, timer, Create(cPath, cName, cLine), enabled);

        //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        //public static TResult Func<TResult>(this ILog log,
        //    bool enabled,
        //    Func<(TResult Result, string Message)> func,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.FuncMessage(func, null, null, timer, Create(cPath, cName, cLine), enabled);

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<(TResult Result, string FinalMessage)> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncMessage(func, parameters, message, timer, Create(cPath, cName, cLine), enabled);


        private static TResult FuncMessage<TResult>(this ILog log,
            Func<(TResult Result, string Message)> func,
            string parameters,
            string message,
            bool timer,
            CodeRef code,
            bool enabled,
            bool logResult = true)
        {
            var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
            var (result, resultMsg) = func();
            if (!enabled) return result;
            return logResult ? l.ReturnAndLog(result, resultMsg) : l.Return(result, resultMsg);
        }


        #endregion

        #region Func Log / Result

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            Func<ILogCall, TResult> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncLogResult(func, null, message, timer, Create(cPath, cName, cLine), enabled);

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<ILogCall, TResult> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncLogResult(func, parameters, message, timer, Create(cPath, cName, cLine), enabled);

        private static TResult FuncLogResult<TResult>(this ILog log,
            Func<ILogCall, TResult> func,
            string parameters,
            string message,
            bool timer,
            CodeRef code,
            bool enabled,
            bool logResult = true)
        {
            var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
            var result = func(l);
            if (!enabled) return result;
            return logResult ? l.ReturnAndLog(result) : l.Return(result);
        }

        #endregion

        #region Func Log Result Message


        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            Func<ILogCall, (TResult Result, string FinalMessage)> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncLogResultMessage(func, null, message, timer,
            Create(cPath, cName, cLine), enabled);

        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
        public static TResult Func<TResult>(this ILog log,
            string parameters,
            Func<ILogCall, (TResult Result, string FinalMessage)> func,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.FuncLogResultMessage(func, parameters, message, timer,
            Create(cPath, cName, cLine), enabled);


        private static TResult FuncLogResultMessage<TResult>(this ILog log,
            Func<ILogCall, (TResult Result, string FinalMessage)> func,
            string parameters,
            string message,
            bool timer,
            CodeRef code,
            bool enabled,
            bool logResult = true)
        {
            var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
            var (result, resultMessage) = func(l);
            if (!enabled) return result;
            return logResult ? l.ReturnAndLog(result, resultMessage) : l.Return(result, resultMessage);
        }

        #endregion

    }
}
