using System;
using System.Collections.Generic;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A logger with special capabilities.
    /// It can take log messages, and chain itself to other loggers. <br/>
    /// If chained, it can broadcast the messages to the other loggers from that time forward.
    /// Basically this is the backbone of Insights.
    /// </summary>
    //[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public partial interface ILog
    {
        /// <summary>
        /// When the log object was created - for rare output scenarios
        /// </summary>
        //[PrivateApi]
        //DateTime Created { get; }

        /// <summary>
        /// A short random ID to differentiate this logger from others.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A unique identifier containing a special XXX.yyyyy[id] name
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Determines if this log should be preserved in the short term.
        /// Like for live-analytics / live-insights.
        /// Default is true, but in certain cases it will default to false.
        /// </summary>
        bool Preserve { get; set; }

        int Depth { get; set; }

        //[PrivateApi]
        //List<Entry> Entries { get; }

        //[PrivateApi]
        //string FullIdentifier { get; }

        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="parent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        //[PrivateApi]
        //void LinkTo(ILog parent, string name = null);

        /// <summary>
        /// Unlink a logger from the parent.
        /// </summary>
        //[PrivateApi]
        //void Unlink();

        //[PrivateApi("not public, created in v13")]
        //ILog Parent { get; }
    }
}