using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class LogCallBaseExtensions
{
    #region DoInTimer

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void DoInTimer(this ILogCall logCall, Action action)
    {
        var skipStartingTimer = logCall?.Timer.IsRunning ?? true;
        if (!skipStartingTimer) logCall.Timer.Start();
        action();
        if (!skipStartingTimer) logCall.Timer.Stop();
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static TResult DoInTimer<TResult>(this ILogCall logCall, Func<TResult> action)
    {
        var skipStartingTimer = logCall?.Timer.IsRunning ?? true;
        if (!skipStartingTimer) logCall.Timer.Start();
        var result = action();
        if (!skipStartingTimer) logCall.Timer.Stop();
        return result;
    }


    #endregion

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static void DoneInternal(this ILogCall logCall, string message)
    {
        if (!(logCall?.Log is Log log)) return;

        //if (!logCall.IsOpen)
        //    log.AddInternal("Log Warning: Wrapper already closed from previous call", null);
        //logCall.IsOpen = false;

        log.WrapDepth--;
        logCall.Entry.AppendResult(message);
        var final = log.AddInternalReuse(null, null);
        final.WrapClose = true;
        final.AppendResult(message);
        if (logCall.Timer == null) return;
        logCall.Timer.Stop();
        logCall.Entry.Elapsed = logCall.Timer.Elapsed;
    }

}