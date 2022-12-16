﻿using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {

        public static ILog SubLogOrNull(this ILog log, string name, bool enabled = true)
        {
            if (log?._RealLog == default || !enabled) return null;
            return new Log(name, log._RealLog);
        }
        

        // 2dm - experimental, don't use yet...
        public static void Wrp(this ILog log,
            Func<LogCall, string> action,
            string parameters = default,
            string message = default,
            bool startTimer = false,
            CodeRef code = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log?._RealLog, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);
            var finalMsg = action(l);
            l.Done(finalMsg);
        }

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
            var l = new LogCall(log?._RealLog, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);
            var finalMsg = action();
            l.Done(finalMsg);
        }

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
            var l = new LogCall<TResult>(log?._RealLog, code ?? new CodeRef(cPath, cName, cLine), false, parameters, message, startTimer);
            var result = action(l);
            return l.Return(result.Result, result.Message);
        }
    }
}
