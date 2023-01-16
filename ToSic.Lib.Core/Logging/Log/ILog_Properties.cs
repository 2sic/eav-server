﻿using System;
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
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, null, timer) : null;
            var result = getter();
            return !enabled ? result : l.ReturnAndLog(result);
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static TProperty Getter<TProperty>(this ILog log,
            Func<(TProperty Result, string Message)> getter,
            bool timer = default,
            bool enabled = true,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall<TProperty>(log, Create(cPath, "get:" + cName, cLine), true, null, null, timer) : null;
            var (result, message) = getter();
            return !enabled ? result : l.Return(result, message);
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static void Setter(this ILog log,
            Action setter,
            bool timer = default,
            bool enabled = true,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = enabled ? new LogCall(log, Create(cPath, "set:" + cName, cLine), true, null, null, timer) : null;
            setter();
            if (!enabled) return;
            l.Done();
        }

        [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
        public static void Setter<TProperty>(this ILog log,
            Func<TProperty> setter,
            bool timer = default,
            bool enabled = true,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall<TProperty>(log, Create(cPath, "set:" + cName + "=", cLine), true, null, null, timer);
            var result = setter();
            if (!enabled) return;
            l.ReturnAndLog(result);
        }
    }
}