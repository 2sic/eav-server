using ToSic.Lib.DI;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Experimental extension, should replace many of the Init-implementations
    /// </summary>
    public static class HasLogInit
    {
        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null.
        ///
        /// todo: this will become obsolete - but I must verify each case.
        /// as long as it's Init - it must be reviewed.
        /// Those that should stay should be changed to linklog...
        /// </summary>
        /// <returns>The same object as started this, to allow chaining</returns>
        public static T Init<T>(this T thingWithLog, ILog parentLog) where T: class, IHasLog 
            => thingWithLog.LinkLog(parentLog, name: null);

        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null. 
        /// </summary>
        /// <returns>The same object as started this, to allow chaining</returns>
        public static T LinkLog<T>(this T thingWithLog, ILog parentLog) where T: class, IHasLog 
            => thingWithLog.LinkLog(parentLog, name: null);

        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null. 
        /// </summary>
        /// <returns>The same object as started this, to allow chaining</returns>
        public static T LinkLog<T>(this T thingWithLog, ILog parentLog, string name, bool forceConnect = false) where T: class, IHasLog
        {
            if (thingWithLog == null) return null;
            if (thingWithLog is ILogShouldNeverConnect && !forceConnect)
                return thingWithLog;

            if (thingWithLog is ILazyInitLog logConnector)
                logConnector.SetLog(parentLog);
            else
            {
                // Connect if possible
                (thingWithLog.Log as Log)?.LinkTo(parentLog, name);

                // If the object needs a call back, give it...
                (thingWithLog as ILogWasConnected)?.LogWasConnected();
            }

            return thingWithLog;
        }

    }
}
