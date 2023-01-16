using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Extension methods for **Actions** (functions which don't return a value).
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP")]
    // ReSharper disable once InconsistentNaming
    public static class ILog_Actions
    {
        /// <summary>
        /// Run code / action and just log that it happened.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="action"></param>
        /// <param name="timer"></param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
        public static void Do(this ILog log,
            Action action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log, Create(cPath, cName, cLine), false, null, null, timer);
            action();
            l.Done();
        }

        /// <summary>
        /// Run code / action and just log that it happened.
        /// This overload also accepts parameters to log and optional message.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="parameters"></param>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="timer"></param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
        public static void Do(this ILog log,
            string parameters,
            Action action,
            bool timer = default,
            string message = null,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log, Create(cPath, cName, cLine), false, parameters, message, timer);
            action();
            l.Done();
        }

        /// <summary>
        /// Run code / action which expects the inner logger as parameter, for further logging.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="action"></param>
        /// <param name="timer"></param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
        public static void Do(this ILog log,
            Action<ILogCall> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var l = new LogCall(log, Create(cPath, cName, cLine), false, null, null, timer);
            action(l);
            l.Done();
        }


        /// <summary>
        /// Do something and the inner call can return a message which will be logged.
        /// The result of the inner call will just be logged.
        /// Ideal for calls which have a lot of logic and want to have a final message as to what case applied
        /// </summary>
        /// <param name="log"></param>
        /// <param name="action">The method called, whose return value will be logged but not passed on</param>
        /// <param name="timer"></param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        [PrivateApi]
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
        public static void Do(this ILog log,
            Func<string> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall(log, Create(cPath, cName, cLine), false, null, null, timer)
                .Done(action());

        /// <summary>
        /// Do something and the inner call can return a message which will be logged.
        /// The result of the inner call will just be logged.
        /// Ideal for calls which have a lot of logic and want to have a final message as to what case applied
        /// </summary>
        /// <param name="log"></param>
        /// <param name="parameters"></param>
        /// <param name="action">The method called, whose return value will be logged but not passed on</param>
        /// <param name="timer"></param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        [PrivateApi]
        [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
        public static void Do(this ILog log,
            string parameters,
            Func<string> action,
            bool timer = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall(log, Create(cPath, cName, cLine), false, parameters, null, timer)
                .Done(action());
    }
}
