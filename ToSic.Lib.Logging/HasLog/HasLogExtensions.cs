namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Experimental extension, should replace many of the Init-implementations
    /// </summary>
    public static class HasLogExtensions
    {
        public static T Init<T>(this T thingWithLog, ILog parentLog) where T: IHasLog
        {
            if (thingWithLog.Log is Log log) log.LinkTo(parentLog);
            return thingWithLog;
        }
        public static T Init<T>(this T thingWithLog, ILog parentLog, string name) where T: IHasLog
        {
            if (thingWithLog.Log is Log log) log.LinkTo(parentLog, name);
            return thingWithLog;
        }
    }
}
