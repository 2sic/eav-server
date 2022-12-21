using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A logger with special capabilities.
    /// It can take log messages, and chain itself to other loggers. <br/>
    /// If chained, it can broadcast the messages to the other loggers from that time forward.
    /// Basically this is the backbone of Insights.
    ///
    /// To add messages/logs of all kinds you must use null-safe extension methods.
    /// It will require you to add the namespace <see cref="Logging"/>.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just FYI")]
    public interface ILog
    {
        /// <summary>
        /// A unique identifier containing a special `Scp.NameOf[id]` name.
        /// This consists of
        /// 
        /// 1. `Scp` Scope - up to 3 characters
        /// 1. `NameOf` Name - up to 6 characters
        /// 1. `id` A random id 2 characters long
        /// </summary>
        string NameId { get; }
    }
}