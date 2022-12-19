using System.Collections.Concurrent;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging.Internals;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Log History for real-time Insights.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IHistory
    {
        /// <summary>
        /// Maximum size of a segment.
        /// Once limit has been reached, it will drop previous logs in that segment.
        /// </summary>
        int SegmentSize { get; set; }

        /// <summary>
        /// All segments, each containing one or more logs.
        /// </summary>
        ConcurrentDictionary<string, FixedSizedQueue<ILog>> Segments { get; }

        [PrivateApi]
        bool Pause { get; set; }

        /// <summary>
        /// Total count of logs which had been added.
        /// This is different from the amount of items, as some could have been removed in the meantime.
        /// </summary>
        int AddCount { get; }

        /// <summary>
        /// Add a log to the current history.
        /// </summary>
        /// <param name="segment">Segment name, like `webapi` or `module`</param>
        /// <param name="log"></param>
        void Add(string segment, ILog log);

        [PrivateApi]
        void ForceAdd(string key, ILog log);
        [PrivateApi]
        void FlushSegment(string segment);
    }
}