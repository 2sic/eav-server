namespace ToSic.Eav.Logging
{

    public static class LogExtensions
    {
        /// <summary>
        /// Attach this log to another parent log, which should also know about events logged here.
        /// </summary>
        /// <param name="hasLog">thing which has a log</param>
        /// <param name="parentLog">The parent log</param>
        public static void LinkLog(this IHasLog hasLog, ILog parentLog) => hasLog.Log.LinkTo(parentLog);
    }
}
