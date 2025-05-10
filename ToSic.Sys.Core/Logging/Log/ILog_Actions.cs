using System.Runtime.CompilerServices;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging;

/// <summary>
/// Extension methods for **Actions** (functions which don't return a value).
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP")]
// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
// ReSharper disable once InconsistentNaming
public static class ILog_Actions
{
    /// <summary>
    /// Run code / action and just log that it happened.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="action"></param>
    /// <param name="message"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <param name="timer"></param>
    /// <param name="enabled"></param>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog? log,
        Action action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
        // ReSharper disable ExplicitCallerInfoArgument
    ) => log.Do(parameters: null, action, timer: timer, enabled: enabled, message: message, cPath: cPath, cName: cName, cLine: cLine);
    // ReSharper restore ExplicitCallerInfoArgument

    /// <summary>
    /// Run code / action and just log that it happened.
    /// This overload also accepts parameters to log and optional message.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="parameters"></param>
    /// <param name="action"></param>
    /// <param name="enabled"></param>
    /// <param name="message"></param>
    /// <param name="timer"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog? log,
        string? parameters,
        Action action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        var l = enabled
            ? new LogCall(log, Create(cPath!, cName!, cLine), false, parameters, message, timer)
            : null;
        action();
        if (enabled) l.Done();
    }

    /// <summary>
    /// Run code / action which expects the inner logger as parameter, for further logging.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="action"></param>
    /// <param name="message"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <param name="timer"></param>
    /// <param name="enabled"></param>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog? log,
        Action<ILogCall> action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
        // ReSharper disable ExplicitCallerInfoArgument
    ) => log.DoLogCall(parameters: null, action, timer: timer, enabled: enabled, message: message, cPath: cPath, cName: cName, cLine: cLine);
    // ReSharper restore ExplicitCallerInfoArgument

    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog log,
        string parameters,
        Action<ILogCall> action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
        // ReSharper disable ExplicitCallerInfoArgument
    ) => log.DoLogCall(parameters, action, timer: timer, enabled: enabled, message: message, cPath: cPath, cName: cName, cLine: cLine);
    // ReSharper restore ExplicitCallerInfoArgument

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private static void DoLogCall(this ILog? log,
        string? parameters,
        Action<ILogCall> action,
        bool timer = default,
        bool enabled = true,
        string? message = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        var l = new LogCall(enabled ? log : null, Create(cPath!, cName!, cLine), false, parameters, message, timer);
        action(l);
        if (enabled) l.Done();
    }

    /// <summary>
    /// Do something and the inner call can return a message which will be logged.
    /// The result of the inner call will just be logged.
    /// Ideal for calls which have a lot of logic and want to have a final message as to what case applied
    /// </summary>
    /// <param name="log"></param>
    /// <param name="action">The method called, whose return value will be logged but not passed on</param>
    /// <param name="timer"></param>
    /// <param name="message"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <param name="enabled"></param>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog? log,
        Func<string> action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
        // ReSharper disable ExplicitCallerInfoArgument
    ) => log.Do(null, action, timer: timer, enabled: enabled, message: message, cPath: cPath, cName: cName, cLine: cLine);
    // ReSharper restore ExplicitCallerInfoArgument


    /// <summary>
    /// Do something and the inner call can return a message which will be logged.
    /// The result of the inner call will just be logged.
    /// Ideal for calls which have a lot of logic and want to have a final message as to what case applied
    /// </summary>
    /// <param name="log"></param>
    /// <param name="parameters"></param>
    /// <param name="action">The method called, whose return value will be logged but not passed on</param>
    /// <param name="timer"></param>
    /// <param name="message"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <param name="enabled"></param>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP but probably final")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void Do(this ILog? log,
        string? parameters,
        Func<string> action,
        bool timer = default,
        bool enabled = true,
        string? message = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        var l = new LogCall(enabled ? log : null, Create(cPath!, cName!, cLine), false, parameters, message, timer);
        var msg = action();
        if (enabled) l.Done(msg);
    }


}