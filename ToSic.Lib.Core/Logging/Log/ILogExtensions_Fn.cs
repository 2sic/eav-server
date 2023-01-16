using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {

        /// <summary>
        /// Log a function call from start up until returning the result.
        /// </summary>
        /// <typeparam name="T">The type of the final result.</typeparam>
        /// <param name="log">The parent <see cref="ILog"/> or <see cref="ILogCall"/> object.</param>
        /// <param name="parameters">Optional parameters used in the call</param>
        /// <param name="message">Optional message</param>
        /// <param name="timer">Enable the timer/stopwatch.</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        /// <returns></returns>
        public static ILogCall<T> Fn<T>(this ILog log,
            string parameters = default,
            string message = default,
            bool timer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall<T>(log, CodeRef.Create(cPath, cName, cLine), false, parameters, message, timer);

        /// <summary>
        /// Log a function call from start up until returning the result.
        /// </summary>
        /// <typeparam name="T">The type of the final result.</typeparam>
        /// <param name="log">The parent <see cref="ILog"/> or <see cref="ILogCall"/> object.</param>
        /// <param name="enabled">Boolean toggle. If false, logging won't happen.</param>
        /// <param name="parameters">Optional parameters used in the call</param>
        /// <param name="message">Optional message</param>
        /// <param name="timer">Enable the timer/stopwatch.</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        /// <returns></returns>
        public static ILogCall<T> Fn<T>(this ILog log,
            bool enabled,
            string parameters = default,
            string message = default,
            bool timer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall<T>(enabled ? log : null, CodeRef.Create(cPath, cName, cLine), false, parameters, message, timer);


        /// <summary>
        /// Log a function call from start up until returning completion - without a result.
        /// </summary>
        /// <param name="log">The parent <see cref="ILog"/> or <see cref="ILogCall"/> object.</param>
        /// <param name="parameters">Optional parameters used in the call</param>
        /// <param name="message">Optional message</param>
        /// <param name="timer">Enable the timer/stopwatch.</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static ILogCall Fn(this ILog log,
            string parameters = default,
            string message = default,
            bool timer = false,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => new LogCall(log, CodeRef.Create(cPath, cName, cLine), false, parameters, message, timer);

        [PrivateApi]
        internal static ILogCall FnCode(this ILog log,
            string parameters = default,
            string message = default,
            bool timer = false,
            CodeRef code = default
        ) => new LogCall(log, code, false, parameters, message, timer);

        [PrivateApi]
        internal static ILogCall<TResult> FnCode<TResult>(this ILog log,
            string parameters = default,
            string message = default,
            bool timer = false,
            CodeRef code = default
        ) => new LogCall<TResult>(log, code, false, parameters, message, timer);

        /// <summary>
        /// Special helper to use a function to create a message, but ignore any errors to avoid problems when only logging.
        /// </summary>
        /// <param name="_">The log object - not used, just for syntax</param>
        /// <param name="messageMaker">Function to generate the message.</param>
        /// <returns></returns>
        [InternalApi_DoNotUse_MayChangeWithoutNotice("will probably be moved elsewhere some day")]
        public static string Try(this ILog _, Func<string> messageMaker)
        {
            try
            {
                return messageMaker.Invoke();
            }
            catch (Exception ex)
            {
                return "LOG: failed to generate from code, error: " + ex.Message;
            }
        }
    }
}
