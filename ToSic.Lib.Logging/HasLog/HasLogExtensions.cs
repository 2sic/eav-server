namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Experimental extension, should replace many of the Init-implementations
    /// </summary>
    public static class HasLogExtensions
    {
        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null. 
        /// </summary>
        /// <returns>The same object as started this, to allow chaining</returns>
        public static T Init<T>(this T thingWithLog, ILog parentLog) where T: IHasLog 
            => thingWithLog.Init(parentLog, name: null);

        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null. 
        /// </summary>
        /// <returns>The same object as started this, to allow chaining</returns>
        public static T Init<T>(this T thingWithLog, ILog parentLog, string name) where T: IHasLog
        {
            if (thingWithLog == null || thingWithLog is ILogShouldNeverConnect)
                return thingWithLog;

            // Connect if possible
            (thingWithLog.Log as Log)?.LinkTo(parentLog, name);

            // If the object needs a call back, give it...
            (thingWithLog as ILogWasConnected)?.LogWasConnected();
            return thingWithLog;
        }

    }
}
