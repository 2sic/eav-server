using System.Diagnostics;

namespace ToSic.Sys.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogCallBaseExtensions
{
    #region DoInTimer

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static void DoInTimer(this ILogCall? logCall, Action action)
    {
        var timerWasAlreadyRunning = logCall is { Timer.IsRunning: true };
        var innerTime = Stopwatch.StartNew();
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Start();

        logCall.A($"{nameof(DoInTimer)}; will consolidate times; previous time was: {logCall?.Timer.ElapsedMilliseconds}ms");

        try
        {
            action();
        }
        finally
        {
            if (!timerWasAlreadyRunning)
                logCall?.Timer.Stop();
            if (logCall?.Entry is { } entry)
                entry.IsTimed = true;
        }
        logCall.A($"{nameof(DoInTimer)}; partial-time: {innerTime.ElapsedMilliseconds}ms");
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TResult DoInTimer<TResult>(this ILogCall? logCall, Func<TResult> action)
    {
        var timerWasAlreadyRunning = logCall is { Timer.IsRunning: true };
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Start();
        try
        {
            return action();
        }
        finally
        {
            if (!timerWasAlreadyRunning)
                logCall?.Timer.Stop();
            if (logCall?.Entry is { } entry)
                entry.IsTimed = true;
        }
    }


    #endregion

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    internal static void DoneInternal(this ILogCall? logCall, string? message)
    {
        if (logCall?.Log is not Log log)
            return;

        var entry = logCall.Entry;
        if (entry?.WrapOpenWasClosed == true)
            return;
        if (entry != null && log.CurrentOperation == entry)
            log.CurrentOperation = entry.ParentOperation;
        log.WrapDepth--;
        entry?.AppendResult(message);
        var final = log.AddInternalReuse(null!, null);
        final.WrapClose = true;
        final.AppendResult(message);
        if (entry != null)
        {
            entry.IsTimed |= logCall.Timer.IsRunning || logCall.Timer.ElapsedTicks > 0;
            logCall.Timer.Stop();
            entry.Elapsed = logCall.Timer.Elapsed;
            entry.Completed = DateTime.Now;
            LogEventBridge.Write(entry);
        }
    }

}
