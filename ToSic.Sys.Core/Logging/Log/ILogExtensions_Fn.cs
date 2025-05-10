using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging;

// ReSharper disable once InconsistentNaming
partial class ILogExtensions
{
    /// <summary>
    /// Log a function call from start up until returning the result.
    /// </summary>
    /// <typeparam name="T">The type of the final result.</typeparam>
    /// <param name="log">The parent <see cref="ILog"/> or <see cref="ILogCall"/> object.</param>
    /// <param name="parameters">Optional parameters used in the call</param>
    /// <param name="message">Optional message</param>
    /// <param name="timer">Enable the timer/stopwatch.</param>
    /// <param name="enabled"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ILogCall<T>? Fn<T>(this ILog? log,
        string? parameters = default,
        string? message = default,
        bool timer = false,
        bool enabled = true,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    ) => enabled
        ? new LogCall<T>(log, CodeRef.Create(cPath!, cName!, cLine), false, parameters, message, timer)
        : null;


    /// <summary>
    /// Log a function call from start up until returning completion - without a result.
    /// </summary>
    /// <param name="log">The parent <see cref="ILog"/> or <see cref="ILogCall"/> object.</param>
    /// <param name="parameters">Optional parameters used in the call</param>
    /// <param name="message">Optional message</param>
    /// <param name="timer">Enable the timer/stopwatch.</param>
    /// <param name="enabled"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ILogCall? Fn(this ILog? log,
        string? parameters = default,
        string? message = default,
        bool timer = false,
        bool enabled = true,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    ) => enabled
        ? new LogCall(log, CodeRef.Create(cPath!, cName!, cLine), false, parameters, message, timer)
        : null;

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static ILogCall FnCode(this ILog? log,
        string? parameters = default,
        string? message = default,
        bool timer = false,
        CodeRef? code = default
    ) => new LogCall(log, code ?? new(), false, parameters, message, timer);

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static ILogCall<TResult> FnCode<TResult>(this ILog? log,
        string? parameters = default,
        string? message = default,
        bool timer = false,
        CodeRef? code = default
    ) => new LogCall<TResult>(log, code ?? new(), false, parameters, message, timer);

    /// <summary>
    /// Special helper to use a function to create a message, but ignore any errors to avoid problems when only logging.
    /// </summary>
    /// <param name="log">The log object - not used, just for syntax</param>
    /// <param name="messageMaker">Function to generate the message.</param>
    /// <param name="errorMessage">Message to show if it fails</param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("will probably be moved elsewhere some day")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string? Try(this ILog? log, Func<string?> messageMaker, string? errorMessage = null)
    {
        try
        {
            return messageMaker.Invoke();
        }
        catch (Exception ex)
        {
            log.Ex(ex);
            return errorMessage ?? "LOG: failed to generate from code";
        }
    }
}