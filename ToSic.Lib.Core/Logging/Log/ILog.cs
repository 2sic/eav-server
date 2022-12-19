using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A logger with special capabilities.
    /// It can take log messages, and chain itself to other loggers. <br/>
    /// If chained, it can broadcast the messages to the other loggers from that time forward.
    /// Basically this is the backbone of Insights.
    ///
    /// Note that just about all APIs to add things to logs etc. are null-safe extension methods, so they are not listed here.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just FYI")]
    public interface ILog
    {
        /// <summary>
        /// A unique identifier containing a special XXX.yyyyy[id] name
        /// </summary>
        string NameId { get; }

        /// <summary>
        /// Determines if this log should be preserved in the short term.
        /// Like for live-analytics / live-insights.
        /// Default is true, but in certain cases it will default to false.
        /// </summary>
        [PrivateApi]
        bool Preserve { get; set; }

        /// <summary>
        /// Special internal property which is only used to ensure Logs and LogCalls both work.
        /// 
        /// Because of the way the logging works, it can be null!
        /// This is because it's possible to pass a null-logger around, to ensure that logging is disabled.
        /// </summary>
        [PrivateApi]
        ILog _RealLog { get; }
    }
}