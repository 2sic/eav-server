using System.Runtime.CompilerServices;
using static ToSic.Lib.Logging.CodeRef;
// ReSharper disable ExplicitCallerInfoArgument

namespace ToSic.Lib.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public static class ILog_Func
{
    #region Func with return value only

    [InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TResult Quick<TResult>(this ILog? log,
        Func<TResult> func,
        bool timer = default,
        bool enabled = true,
        string? parameters = default,
        string? message = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        //  #duplicateFuncResult<TResult> - Make sure we keep it in sync
        // This section is a duplicate of other implementations
        // We're keeping the code duplicate so the call stack doesn't get too deep when debugging
        var l = enabled
            ? log.FnCode<TResult>(parameters, message, timer, Create(cPath!, cName!, cLine))
            : null;
        var result = func();
        if (!enabled)
            return result;
        return l.ReturnAndLog(result);

    }


    ///// <summary>
    ///// Call a method to get the result and intercept / log what was returned
    ///// </summary>
    ///// <returns></returns>
    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog log,
    //    string parameters,
    //    Func<TResult> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //)
    //{
    //    //  #duplicateFuncResult<TResult> - Make sure we keep it in sync
    //    // This section is a duplicate of other implementations
    //    // We're keeping the code duplicate so the call stack doesn't get too deep when debugging
    //    var l = enabled ? log.FnCode<TResult>(parameters, message, timer, Create(cPath!, cName!, cLine)) : null;
    //    var result = func();
    //    return enabled
    //        ? l.ReturnAndLog(result)
    //        : result;
    //}

    //private static TResult FuncResult<TResult>(this ILog log,
    //    Func<TResult> func,
    //    string parameters,
    //    string message,
    //    bool timer,
    //    CodeRef code,
    //    bool enabled,
    //    bool logResult = true)
    //{
    //    var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
    //    var result = func();
    //    if (!enabled) return result;
    //    return logResult ? l.ReturnAndLog(result) : l.Return(result);
    //}


    #endregion

    #region Func with Result/Message returned

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog log,
    //    Func<(TResult Result, string FinalMessage)> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //)
    //{
    //    const string? parameters = null;
    //    // #duplicateFuncMessage<TResult>
    //    // This section is a duplicate of other implementations
    //    // We're keeping the code duplicate so the call stack doesn't get too deep when debugging
    //    var l = enabled
    //        ? log.FnCode<TResult>(parameters, message, timer, Create(cPath!, cName!, cLine))
    //        : null;
    //    var (result, resultMsg) = func();
    //    return enabled
    //        ? l.ReturnAndLog(result, resultMsg)
    //        : result;
    //}

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //public static TResult Func<TResult>(this ILog log,
    //    bool enabled,
    //    Func<(TResult Result, string Message)> func,
    //    bool timer = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //) => log.FuncMessage(func, null, null, timer, Create(cPath!, cName!, cLine), enabled);

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog log,
    //    string parameters,
    //    Func<(TResult Result, string FinalMessage)> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //)
    //{
    //    // #duplicateFuncMessage<TResult>
    //    // This section is a duplicate of other implementations
    //    // We're keeping the code duplicate so the call stack doesn't get too deep when debugging
    //    var l = enabled ? log.FnCode<TResult>(parameters, message, timer, Create(cPath!, cName!, cLine)) : null;
    //    var (result, resultMsg) = func();
    //    return enabled ? l.ReturnAndLog(result, resultMsg) : result;
    //}


    //private static TResult FuncMessage<TResult>(this ILog log,
    //    Func<(TResult Result, string Message)> func,
    //    string parameters,
    //    string message,
    //    bool timer,
    //    CodeRef code,
    //    bool enabled,
    //    bool logResult = true)
    //{
    //    var l = enabled ? log.FnCode<TResult>(parameters, message, timer, code) : null;
    //    var (result, resultMsg) = func();
    //    if (!enabled) return result;
    //    return logResult ? l.ReturnAndLog(result, resultMsg) : l.Return(result, resultMsg);
    //}


    #endregion

    #region Func Log / Result

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult? Func<TResult>(this ILog log,
    //    Func<ILogCall?, TResult?> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //) => log.FuncLogResult(func, null, message, timer, Create(cPath!, cName!, cLine), enabled);

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog? log,
    //    string parameters,
    //    Func<ILogCall?, TResult> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //) => log.FuncLogResult(func, parameters, message, timer, Create(cPath!, cName!, cLine), enabled);

    //// Experimental - try to force inline when compiling, to reduce call stack
    //// https://stackoverflow.com/questions/12303924/how-to-force-inline-functions-in-c
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //private static TResult FuncLogResult<TResult>(this ILog? log,
    //    Func<ILogCall?, TResult> func,
    //    string? parameters,
    //    string? message,
    //    bool timer,
    //    CodeRef code,
    //    bool enabled)
    //{
    //    var l = enabled
    //        ? log.FnCode<TResult>(parameters, message, timer, code)
    //        : null;
    //    var result = func(l);
    //    return enabled
    //        ? l.ReturnAndLog(result)
    //        : result;
    //}

    #endregion

    #region Func Log Result Message


    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog log,
    //    Func<ILogCall?, (TResult Result, string FinalMessage)> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //) => log.FuncLogResultMessage(func, null, message, timer,
    //    Create(cPath!, cName!, cLine), enabled);

    //[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP, not final")]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //public static TResult Func<TResult>(this ILog log,
    //    string parameters,
    //    Func<ILogCall?, (TResult Result, string FinalMessage)> func,
    //    bool timer = default,
    //    bool enabled = true,
    //    string? message = default,
    //    [CallerFilePath] string? cPath = default,
    //    [CallerMemberName] string? cName = default,
    //    [CallerLineNumber] int cLine = default
    //) => log.FuncLogResultMessage(func, parameters, message, timer,
    //    Create(cPath!, cName!, cLine), enabled);


    //// Experimental - try to force inline when compiling, to reduce call stack
    //// https://stackoverflow.com/questions/12303924/how-to-force-inline-functions-in-c
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[ShowApiWhenReleased(ShowApiMode.Never)]
    //private static TResult FuncLogResultMessage<TResult>(this ILog log,
    //    Func<ILogCall?, (TResult Result, string FinalMessage)> func,
    //    string? parameters,
    //    string? message,
    //    bool timer,
    //    CodeRef code,
    //    bool enabled,
    //    bool logResult = true)
    //{
    //    var l = enabled
    //        ? log.FnCode<TResult>(parameters, message, timer, code)
    //        : null;
    //    var (result, resultMessage) = func(l);
    //    if (!enabled) return result;
    //    return logResult
    //        ? l.ReturnAndLog(result, resultMessage)
    //        : l.Return(result, resultMessage);
    //}

    #endregion

}