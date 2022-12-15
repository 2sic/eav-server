namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A logger with special capabilities.
    /// It can take log messages, and chain itself to other loggers. <br/>
    /// If chained, it can broadcast the messages to the other loggers from that time forward.
    /// Basically this is the backbone of Insights.
    /// </summary>
    //[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
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
        bool Preserve { get; set; }
    }
}