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
        return !enabled
            ? result
            : l.ReturnAndLog(result);
    }


    #endregion

}