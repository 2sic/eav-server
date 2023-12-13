using System.Collections.Concurrent;
using System.Linq;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging.Internals;

namespace ToSic.Lib.Logging;

/// <inheritdoc />
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogStoreLive : ILogStoreLive
{
    [PrivateApi]
    public int MaxItems => LogConstants.LiveStoreMaxItems;

    /// <inheritdoc />
    public int SegmentSize
    {
        get => _segmentSize; 
        set => _segmentSize = value;
    } 
    private static int _segmentSize = LogConstants.LiveStoreSegmentSize;

    /// <inheritdoc />
    public ConcurrentDictionary<string, FixedSizedQueue<LogStoreEntry>> Segments => StaticSegments;
    private static readonly ConcurrentDictionary<string, FixedSizedQueue<LogStoreEntry>> StaticSegments = new();

    #region Pause

    [PrivateApi]
    public bool Pause
    {
        get => _pause;
        set
        {
            _pause = value;
            AddCount = 0;
        }
    }
    private static bool _pause;

    #endregion

    /// <inheritdoc />
    public int AddCount { get; private set; }

    #region Add

    /// <inheritdoc />
    public LogStoreEntry Add(string segment, ILog log) => AddInternal(segment, log, false);

    [PrivateApi("shouldn't be visible outside")]
    public LogStoreEntry ForceAdd(string key, ILog log) => AddInternal(key, log, true);

    [PrivateApi]
    private LogStoreEntry AddInternal(string key, ILog log, bool force)
    {
        // Check exit clauses if not forced
        if (!force)
        {
            // only add if not paused
            if (Pause) return null;

            // don't keep in journal if it shouldn't be preserved
            if ((log as Log)?.Preserve != true) return null;
        }

        // auto-pause after 1000 logs were run through this, till someone decides to unpause again
        if (AddCount++ > MaxItems) Pause = true;

        // make sure we have a queue
        if (!Segments.ContainsKey(key))
            Segments.TryAdd(key, new FixedSizedQueue<LogStoreEntry>(SegmentSize));

        // add the current item if it's not already in the queue
        var entry = new LogStoreEntry { Log = log };
        if (Segments.TryGetValue(key, out var queue) && queue.ToArray().All(x => x.Log != log))
            queue.Enqueue(entry);
        return entry;
    }


    #endregion

    [PrivateApi]
    public void FlushSegment(string segment)
    {
        if (Segments.ContainsKey(segment))
            Segments.TryRemove(segment, out var _);
    }
}