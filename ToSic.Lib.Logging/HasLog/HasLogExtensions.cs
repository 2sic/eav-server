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
        /// <typeparam name="T"></typeparam>
        /// <param name="thingWithLog"></param>
        /// <param name="parentLog"></param>
        /// <returns></returns>
        public static T Init<T>(this T thingWithLog, ILog parentLog) where T: IHasLog
        {
            return thingWithLog.Init(parentLog, name: null);
            //(thingWithLog?.Log as Log)?.LinkTo(parentLog);
            ////if (thingWithLog.Log is Log log) log.LinkTo(parentLog);
            //(thingWithLog as ILogWasConnected)?.LogWasConnected();
            //return thingWithLog;
        }

        public static T Init<T>(this T thingWithLog, ILog parentLog, string name) where T: IHasLog
        {
            if (thingWithLog is ILogShouldNeverConnect)
                return thingWithLog;

            (thingWithLog?.Log as Log)?.LinkTo(parentLog, name);
            //if (thingWithLog.Log is Log log) log.LinkTo(parentLog, name);
            (thingWithLog as ILogWasConnected)?.LogWasConnected();
            return thingWithLog;
        }

        public static void LogReInit<T>(this T thingWithLog,
            string name,
            ILog parentLog,
            string initialMessage,
            CodeRef code
        ) where T : IHasLog
        {
            if (thingWithLog?.Log == null) 
                return;

            // late-init case, where the log was already created - just reconfigure keeping what was in it
            thingWithLog.Log.Rename(name);
            thingWithLog.Init(parentLog);
            if (initialMessage == null) return;
            thingWithLog.Log.A(initialMessage, code?.Path, code?.Name, code?.Line ?? 0);
        }
    }
}
