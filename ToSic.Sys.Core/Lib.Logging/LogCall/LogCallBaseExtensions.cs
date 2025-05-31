namespace ToSic.Lib.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogCallBaseExtensions
{
    #region DoInTimer

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static void DoInTimer(this ILogCall? logCall, Action action)
    {
        var timerWasAlreadyRunning = logCall is { Timer.IsRunning: true };
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Start();
        action();
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Stop();
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TResult DoInTimer<TResult>(this ILogCall? logCall, Func<TResult> action)
    {
        var timerWasAlreadyRunning = logCall is { Timer.IsRunning: true };
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Start();
        var result = action();
        if (!timerWasAlreadyRunning)
            logCall?.Timer.Stop();
        return result;
    }


    #endregion

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    internal static void DoneInternal(this ILogCall? logCall, string? message)
    {
        if (logCall?.Log is not Log log)
            return;

        //if (!logCall.IsOpen)
        //    log.AddInternal("Log Warning: Wrapper already closed from previous call", null);
        //logCall.IsOpen = false;

        log.WrapDepth--;
        logCall.Entry?.AppendResult(message);
        var final = log.AddInternalReuse(null!, null);
        final.WrapClose = true;
        final.AppendResult(message);
        if (logCall?.Timer == null)
            return;
        logCall.Timer.Stop();
        if (logCall.Entry != null)
            logCall.Entry.Elapsed = logCall.Timer.Elapsed;
    }

}