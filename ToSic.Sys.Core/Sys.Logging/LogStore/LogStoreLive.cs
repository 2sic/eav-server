namespace ToSic.Sys.Logging;

/// <summary>Admission and read API for the single local Insights store.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LogStoreLive : ILogStoreLive
{
    public const string StoreConfigurationKey = "Logging:2sxc:Store";
    private readonly InsightsLogStore _insights;
    private readonly InsightsLoggerProvider _provider;
    private readonly object _sync = new();
    public int MaxItems => LogConstants.LiveStoreMaxItems;
    public string Status => _insights.Status;

    public LogStoreLive(InsightsLogStore? insights = null, InsightsLoggerProvider? provider = null)
    {
        _insights = insights ?? new();
        _provider = provider ?? new(_insights);
        // Capture is available as soon as the store is first resolved. The host replaces this
        // direct sink with its factory-backed sink after ILoggerFactory is available.
        LogEventBridge.SetSink(_provider);
    }

    public int SegmentSize
    {
        get => _segmentSize;
        set
        {
            if (value < 1 || value > InsightsLogStore.MaxLogs)
                throw new ArgumentOutOfRangeException(nameof(value));
            _segmentSize = value;
            _provider.SegmentSize = value;
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

    /// <summary>Reports an obsolete store selection; local capture always uses the ILogger store.</summary>
    public string Configure(string? obsoleteStore)
        => string.IsNullOrWhiteSpace(obsoleteStore)
            ? Status
            : $"{StoreConfigurationKey}={obsoleteStore} is obsolete; using the ILogger store. {Status}";

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
            var entry = new LogStoreEntry { Log = realLog, Segment = key };
            var preAdmissionExecutionId = LogOperationContext.Latest?.ExecutionId;
            var canAdopt = preAdmissionExecutionId != null && !_insights.Knows(preAdmissionExecutionId, []);
            realLog.CurrentAdmission = entry;
            realLog.LatestExecutionId = entry.ExecutionId;
            var adoption = canAdopt
                ? LogOperationContext.AdoptCurrent(realLog, preAdmissionExecutionId!, entry.ExecutionId, entry.Sequence)
                : null;
            PublishAdmission(key, entry, canAdopt ? preAdmissionExecutionId : null, adoption);
            if (++AddCount >= MaxItems)
                _pause = true;
            return entry;
        }
    }

    private void PublishAdmission(string segment, LogStoreEntry entry, string? preAdmissionExecutionId,
        (long OperationId, int Depth)? adoption)
    {
        if (entry.Log is not Log log)
            return;
        LogEventBridge.Replay(log);
        var admission = LogEvent.ForLog(log);
        var properties = admission.Properties.SetItem(LogExecution.SourceLogIdKey, log.LogId);
        if (preAdmissionExecutionId != null && !_insights.Knows(preAdmissionExecutionId, []))
            properties = properties.SetItem(LogExecution.PreAdmissionExecutionIdKey, preAdmissionExecutionId);
        LogEventBridge.Write(admission with
        {
            LogId = entry.ExecutionId,
            Ancestors = [],
            Properties = properties,
            Kind = "Admission",
            Segment = segment,
            OperationId = adoption?.OperationId,
            Depth = adoption?.Depth ?? 0,
        });
        entry.PublishSpecs();
    }

    public IReadOnlyDictionary<string, int> SegmentCounts() => _insights.SegmentCounts();

    public IReadOnlyList<LogSnapshot> Snapshot(string segment) => _insights.Snapshot(segment);

    public LogSnapshot? Snapshot(ILog? log)
    {
        if (log.GetRealLog() is not Log typed)
            return null;
        return typed.LatestExecutionId is { } executionId && _insights.Find(executionId) is { Entries.Length: > 0 } current
            ? current
            : _insights.Find(typed.LogId);
    }

    public void FlushSegment(string segment) => _insights.Flush(segment);
}
