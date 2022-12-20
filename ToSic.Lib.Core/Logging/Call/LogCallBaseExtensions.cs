using System;

namespace ToSic.Lib.Logging
{
    public static class LogCallBaseExtensions
    {
        #region DoInTimer

        public static void DoInTimer(this LogCallBase logCall, Action action)
        {
            var skipOwnTimer = logCall?.Stopwatch.IsRunning ?? true;
            if (!skipOwnTimer) logCall.Stopwatch.Start();
            action();
            if (!skipOwnTimer) logCall.Stopwatch.Stop();
        }

        public static TResult DoInTimer<TResult>(this LogCallBase logCall, Func<TResult> action)
        {
            var skipOwnTimer = logCall?.Stopwatch.IsRunning ?? true;
            if (!skipOwnTimer) logCall.Stopwatch.Start();
            var result = action();
            if (!skipOwnTimer) logCall.Stopwatch.Stop();
            return result;
        }


        #endregion


        internal static void DoneInternal(this LogCallBase logCall, string message)
        {
            if (!(logCall?.Log is Log log)) return;

            if (!logCall.IsOpen)
                log.AddInternal("Log Warning: Wrapper already closed from previous call", null);
            logCall.IsOpen = false;

            log.WrapDepth--;
            logCall.Entry.AppendResult(message);
            var final = log.AddInternalReuse(null, null);
            final.WrapClose = true;
            final.AppendResult(message);
            if (logCall.Stopwatch == null) return;
            logCall.Stopwatch.Stop();
            logCall.Entry.Elapsed = logCall.Stopwatch.Elapsed;
        }

    }
}
