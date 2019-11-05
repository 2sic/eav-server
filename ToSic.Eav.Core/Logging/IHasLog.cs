using ToSic.Eav.Documentation;

namespace ToSic.Eav.Logging
{
    /// <summary>
    /// Objects which can log their activity, and share their log with other objects in the chain to produce extensive internal logging.
    /// </summary>
    [PublicApi]
    public interface IHasLog
    {
        /// <summary>
        /// The log object which contains the log and can add more logs to the list.
        /// </summary>
        ILog Log { get; }

        /// <summary>
        /// Attach this log to another parent log, which should also know about events logged here.
        /// </summary>
        /// <param name="parentLog">The parent log</param>
        void LinkLog(ILog parentLog);
    }
}