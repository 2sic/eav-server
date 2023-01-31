using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Extension methods for property getters and setters.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still experimental")]
    // ReSharper disable once InconsistentNaming
    public static class ILog_Properties
    {

        /// <summary>
        /// Short wrapper for PropertyGet calls which only return the value.
        /// </summary>
        /// <typeparam name="TProperty">Type of return value</typeparam>
        /// <returns></returns>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static TProperty Getter<TProperty>(this ILog log,
            Func<TProperty> getter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, message, timer) : null;
            var result = getter();
            return !enabled ? result : l.ReturnAndLog(result);
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static TProperty Getter<TProperty>(this ILog log,
            Func<ILogCall, TProperty> getter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, message, timer) : null;
            var result = getter(l);
            return !enabled ? result : l.ReturnAndLog(result);
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static TProperty Getter<TProperty>(this ILog log,
            Func<ILogCall, (TProperty Result, string Message)> getter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, message, timer) : null;
            var result = getter(l);
            return !enabled ? result.Result : l.Return(result.Result, result.Message);
        }


        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static TProperty Getter<TProperty>(this ILog log,
            Func<(TProperty Result, string FinalMessage)> getter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, message, timer) : null;
            var (result, finalMsg) = getter();
            return !enabled ? result : l.Return(result, finalMsg);
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static void Setter(this ILog log,
            Action setter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall(log, Create(cPath, "set:" + cName, cLine), true, null, message, timer) : null;
            setter();
            if (!enabled) return;
            l.Done();
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static void Setter<TProperty>(this ILog log,
            Func<TProperty> setter,
            bool timer = default,
            bool enabled = true,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall<TProperty>(log, Create(cPath, "set:" + cName + "=", cLine), true, null, message, timer);
            var result = setter();
            if (!enabled) return;
            l.ReturnAndLog(result);
        }
    }
}
