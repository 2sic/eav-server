using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

/// <summary>Bounded storage owned by DI. Only the ILogger provider writes event data here.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class InsightsLogStore
{
    public const int MaxLogs = 500;
    public const int MaxSegments = 64;
    public const int MaxEntriesPerLog = 2048;
    public const int MaxTextLength = 4096;
    public const int MaxProperties = 32;
    public const long MaxEstimatedBytes = 16 * 1024 * 1024;
    public bool Enabled { get; internal set; }

    // ponytail: one lock for the bounded diagnostic buffer; partition only if profiling warrants it.
    private readonly object _sync = new();
    private readonly Dictionary<string, Bundle> _logs = new();
    private readonly Dictionary<string, List<string>> _segments = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _order = new();
    private long _bytes;
    private long _dropped;
    private long _evicted;

    public string Status
    {
        get
        {
            lock (_sync)
                return $"ILogger store: {_logs.Count}/{MaxLogs} logs, ~{_bytes / 1024:N0} KB/{MaxEstimatedBytes / 1024:N0} KB budget; {_dropped} dropped events, {_evicted} evicted logs";
        }
    }

    internal void Write(LogEvent data, int segmentSize)
    {
        if (!Enabled)
            return;
        lock (_sync)
        {
            if (data.Kind == "Admission" && data.Segment != null)
            {
                Admit(data, segmentSize);
                return;
            }
            if (data.Kind == "Specs" && data.Segment != null)
            {
                if (_logs.TryGetValue(data.LogId, out var bundle) && bundle.Specs.TryGetValue(data.Segment, out var specs))
                {
                    var merged = specs.SetItems(data.Properties.Take(MaxProperties)).Take(MaxProperties)
                        .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
                    _bytes += Measure(merged) - Measure(specs);
                    bundle.Specs[data.Segment] = merged;
                    EnforceBudget();
                }
                return;
            }

            foreach (var id in data.Ancestors.Insert(0, data.LogId))
            {
                if (!_logs.TryGetValue(id, out var bundle))
                    continue;
                var exists = bundle.Entries.TryGetValue(data.Sequence, out var old);
                if (!exists && bundle.Entries.Count >= MaxEntriesPerLog)
                {
                    bundle.Dropped++;
                    _dropped++;
                    continue;
                }
                // Replays after late attachment must not erase completed data or exception details.
                if (old != null)
                    data = data with
                    {
                        ExceptionType = data.ExceptionType ?? old.ExceptionType,
                        ExceptionText = data.ExceptionText ?? old.ExceptionText,
                        Properties = old.Properties.SetItems(data.Properties)
                            .Take(MaxProperties)
                            .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
                    };
                if (old?.WrapOpenWasClosed == true && !data.WrapOpenWasClosed)
                    continue;
                var size = Measure(data);
                var delta = size - (old == null ? 0 : Measure(old));
                bundle.Bytes += delta;
                _bytes += delta;
                bundle.Entries[data.Sequence] = data;
                if (data.Properties.ContainsKey("2sxc.Truncated") && old == null)
                    bundle.Truncated++;
            }
            EnforceBudget();
        }
    }

    private void Admit(LogEvent data, int segmentSize)
    {
        var segment = data.Segment!;
        if (!_segments.TryGetValue(segment, out var members))
        {
            if (_segments.Count >= MaxSegments)
            {
                _dropped++;
                return;
            }
            _segments[segment] = members = [];
        }
        if (!_logs.TryGetValue(data.LogId, out var bundle))
        {
            _logs[data.LogId] = bundle = new(data.LogId, data.Created);
            _order.AddLast(data.LogId);
            _bytes += 256;
        }
        if (members.Contains(data.LogId))
            return;
        members.Add(data.LogId);
        bundle.Specs[segment] = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase);
        while (members.Count > segmentSize)
            RemoveMembership(segment, members[0]);
        EnforceBudget();
    }

    private void EnforceBudget()
    {
        while (_order.First != null && (_logs.Count > MaxLogs || _bytes > MaxEstimatedBytes))
        {
            var id = _order.First.Value;
            foreach (var segment in _logs[id].Specs.Keys.ToArray())
                RemoveMembership(segment, id);
            _evicted++;
        }
    }

    private void RemoveMembership(string segment, string id)
    {
        _segments[segment].Remove(id);
        var bundle = _logs[id];
        _bytes -= Measure(bundle.Specs[segment]);
        bundle.Specs.Remove(segment);
        if (bundle.Specs.Count != 0)
            return;
        _bytes -= bundle.Bytes + 256;
        _logs.Remove(id);
        _order.Remove(id);
    }

    public void Flush(string segment)
    {
        lock (_sync)
        {
            if (!_segments.TryGetValue(segment, out var members))
                return;
            foreach (var id in members.ToArray())
                RemoveMembership(segment, id);
            _segments.Remove(segment);
        }
    }

    public IReadOnlyDictionary<string, int> SegmentCounts()
    {
        lock (_sync)
            return _segments.ToDictionary(p => p.Key, p => p.Value.Count);
    }

    public IReadOnlyList<LogSnapshot> Snapshot(string segment)
    {
        lock (_sync)
            return !_segments.TryGetValue(segment, out var members) ? []
                : members.Select(id => Snapshot(_logs[id], segment)).ToArray();
    }

    public LogSnapshot? Find(string logId)
    {
        lock (_sync)
        {
            if (_logs.TryGetValue(logId, out var bundle))
                return Snapshot(bundle, null);
            var entries = _logs.Values.SelectMany(b => b.Entries.Values)
                .Where(e => e.LogId == logId || e.Ancestors.Contains(logId))
                .GroupBy(e => e.Sequence).Select(g => g.First()).OrderBy(e => e.Sequence).ToImmutableArray();
            return entries.Length == 0 ? null : new LogSnapshot
            {
                LogId = logId, Created = entries[0].Created, Entries = entries,
                EstimatedBytes = entries.Sum(Measure),
            };
        }
    }

    private static LogSnapshot Snapshot(Bundle bundle, string? segment) => new()
    {
        LogId = bundle.Id, Created = bundle.Created,
        Entries = bundle.Entries.Values.OrderBy(e => e.Sequence).ToImmutableArray(),
        Specs = bundle.Specs[segment ?? bundle.Specs.Keys.First()],
        EstimatedBytes = bundle.Bytes + bundle.Specs.Values.Sum(Measure),
        DroppedEntries = bundle.Dropped, TruncatedEntries = bundle.Truncated,
    };

    private static long Measure(IEnumerable<KeyValuePair<string, string>> properties)
        => properties.Sum(p => 64L + 2L * (p.Key.Length + p.Value.Length));

    private static long Measure(LogEvent e) => 512L + 2L *
        ((e.Message?.Length ?? 0) + (e.Result?.Length ?? 0) + e.Source.Length + e.ShortSource.Length
         + (e.Code?.Path?.Length ?? 0) + (e.Code?.Name?.Length ?? 0) + (e.ExceptionText?.Length ?? 0)
         + (e.ExceptionType?.Length ?? 0) + e.Ancestors.Sum(id => id.Length)) + Measure(e.Properties);

    private sealed class Bundle(string id, DateTime created)
    {
        public string Id { get; } = id;
        public DateTime Created { get; } = created;
        public Dictionary<long, LogEvent> Entries { get; } = new();
        public Dictionary<string, ImmutableDictionary<string, string>> Specs { get; } = new(StringComparer.OrdinalIgnoreCase);
        public long Bytes;
        public int Dropped;
        public int Truncated;
    }
}
