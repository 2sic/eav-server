using ToSic.Lib.DI;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Extension to objects having a Log, to connect them to parent logs.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    // ReSharper disable once InconsistentNaming
    public static class IHasLogExtensions
    {
        /// <summary>
        /// Null-Safe method to link logs together. Both the parent and the Log could be null. 
        /// </summary>
        /// <param name="thingWithLog">Object which is an IHasLog</param>
        /// <param name="parentLog">Log to connect to</param>
        /// <param name="forceConnect">Force connect the logs, even if it's an <see cref="ILogShouldNeverConnect"/></param>
        /// <returns>The same object as started this, to allow chaining</returns>
        [PrivateApi]
        public static T LinkLog<T>(this T thingWithLog, ILog parentLog, /*string name,*/ bool forceConnect = false) where T: class, IHasLog
        {
            if (thingWithLog == null) return null;
            if (thingWithLog is ILogShouldNeverConnect && !forceConnect)
                return thingWithLog;

            if (thingWithLog is ILazyInitLog logConnector)
                logConnector.SetLog(parentLog);
            else
            {
                // Connect if possible
                (thingWithLog.Log as Log)?.LinkTo(parentLog/*, name*/);

                // If the object needs a call back, give it...
                (thingWithLog as ILogWasConnected)?.LogWasConnected();
            }

            return thingWithLog;
        }

    }
}
