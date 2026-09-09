using System.Collections.Concurrent;

namespace ToSic.Sys.Logging;

/// <summary>Compatibility admission API and read selection; ILogger owns the new storage path.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LogStoreLive(InsightsLogStore? insights = null, InsightsLoggerProvider? provider = null) : ILogStoreLive
{
    public const string StoreConfigurationKey = "Logging:2sxc:Store";
    private readonly InsightsLogStore _insights = insights ?? new();
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<string, FixedSizedQueue<LogStoreEntry>> _segments = new();
    public int MaxItems => LogConstants.LiveStoreMaxItems;
    public LogStoreMode Mode { get; private set; }
    public string Status => Mode == LogStoreMode.Legacy ? "Legacy store" : _insights.Status;
    public int SegmentSize
    {
        get => _segmentSize;
        set
        {
            if (value < 1 || value > InsightsLogStore.MaxLogs)
                throw new ArgumentOutOfRangeException(nameof(value));
            _segmentSize = value;
            if (provider != null)
                provider.SegmentSize = value;
        }
    }
    private int _segmentSize = LogConstants.LiveStoreSegmentSize;

    public bool Pause
    {
        get { lock (_sync) return _pause; }
        set
        {
            lock (_sync)
            {
                _pause = value;
                AddCount = 0;
            }
        }
    }
    private bool _pause;
    public int AddCount { get; private set; }

    /// <summary>Call once at startup, after installing the bridge sink.</summary>
    public string Configure(string? mode, bool bridgeEnabled)
    {
        if (!Enum.TryParse(mode ?? nameof(LogStoreMode.Legacy), true, out LogStoreMode selected)
            || !Enum.IsDefined(typeof(LogStoreMode), selected))
            return "Unknown logging store; retaining Legacy.";
        if (selected != LogStoreMode.Legacy && !bridgeEnabled)
            return "ILogger store requires Logging:2sxc:Enabled=true; retaining Legacy.";
        lock (_sync)
        {
            Mode = selected;
            _insights.Enabled = selected != LogStoreMode.Legacy;
            if (!_insights.Enabled)
                return Status;
            // Bootstrap admissions may precede installation of the host's logger factory.
            foreach (var segment in _segments)
                foreach (var entry in segment.Value.ToArray())
                    PublishAdmission(segment.Key, entry);
            if (Mode == LogStoreMode.ILogger)
                _segments.Clear();
            return Status;
        }
    }

    public LogStoreEntry? Add(string segment, ILog log) => AddInternal(segment, log, false);
    public LogStoreEntry? ForceAdd(string key, ILog log) => AddInternal(key, log, true);

    private LogStoreEntry? AddInternal(string key, ILog log, bool force)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 256)
            throw new ArgumentException("A segment name of at most 256 characters is required.", nameof(key));
        if (log.GetRealLog() is not Log realLog)
            return null;
        lock (_sync)
        {
            if (!force && (_pause || !realLog.Preserve))
                return null;
            LogStoreEntry? entry = null;
            if (Mode != LogStoreMode.ILogger)
            {
                var queue = _segments.GetOrAdd(key, _ => new(SegmentSize));
                entry = queue.ToArray().FirstOrDefault(e => e.Log == realLog);
                if (entry == null)
                    queue.Enqueue(entry = new() { Log = realLog, Segment = key });
            }
            entry ??= new() { Log = realLog, Segment = key };
            if (Mode != LogStoreMode.Legacy)
                PublishAdmission(key, entry);
            if (++AddCount >= MaxItems)
                _pause = true;
            return entry;
        }
    }

    private static void PublishAdmission(string segment, LogStoreEntry entry)
    {
        if (entry.Log is not Log log)
            return;
        LogEventBridge.Write(LogEvent.ForLog(log) with { Kind = "Admission", Segment = segment });
        LogEventBridge.Replay(log);
        entry.PublishSpecs();
    }

    public IReadOnlyDictionary<string, int> SegmentCounts() => Mode == LogStoreMode.Legacy
        ? _segments.ToDictionary(p => p.Key, p => p.Value.Count)
        : _insights.SegmentCounts();

    public IReadOnlyList<LogSnapshot> Snapshot(string segment)
    {
        if (Mode != LogStoreMode.Legacy)
            return _insights.Snapshot(segment);
        return !_segments.TryGetValue(segment, out var entries) ? []
            : entries.ToArray().Where(e => e.Log is Log)
                .Select(e => LogSnapshot.FromLegacy((Log)e.Log!, e.Specs)).ToArray();
    }

    public LogSnapshot? Snapshot(ILog? log)
    {
        if (log.GetRealLog() is not Log typed)
            return null;
        return Mode == LogStoreMode.Legacy
            ? LogSnapshot.FromLegacy(typed)
            : _insights.Find(typed.LogId);
    }

    public void FlushSegment(string segment)
    {
        lock (_sync)
        {
            _segments.TryRemove(segment, out _);
            _insights.Flush(segment);
        }
    }
}

/// <summary>Startup selection. Compare retains both stores and renders the ILogger snapshot.</summary>
[PrivateApi]
public enum LogStoreMode { Legacy, Compare, ILogger }
