namespace ToSic.Eav.Logging
{
    /// <summary>
    /// Experimental extension, should replace many of the Init-implementations
    /// </summary>
    public static class IHasLogExtensions
    {
        public static T Init<T>(this T thingWithLog, ILog parentLog) where T: IHasLog
        {
            (thingWithLog.Log as Simple.Log)?.LinkTo(parentLog);
            return thingWithLog;
        }
        public static T Init<T>(this T thingWithLog, ILog parentLog, string name) where T: IHasLog
        {
            (thingWithLog.Log as Simple.Log)?.LinkTo(parentLog, name);
            return thingWithLog;
        }
    }
}
