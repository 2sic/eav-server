using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Objects which can log their activity, and share their log with other objects in the chain to produce extensive internal logging.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IHasLog
    {
        /// <summary>
        /// The log object which contains the log and can add more logs to the list.
        /// </summary>
        ILog Log { get; }

    }
}