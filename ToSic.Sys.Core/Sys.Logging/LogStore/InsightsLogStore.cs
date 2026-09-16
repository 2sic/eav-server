using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ToSic.Sys.Logging;

/// <summary>Bounded storage owned by DI. Only the ILogger provider writes event data here.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class InsightsLogStore
{
    public const int MaxLogs = 500;
    public const int MaxSegments = 64;
    public const int MaxEntriesPerLog = 4096;
    internal const int MaxPendingEntries = MaxEntriesPerLog;
    public const int MaxTextLength = 4096;
    public const int MaxProperties = 32;
    public const long MaxEstimatedBytes = 16 * 1024 * 1024;
    public const string TruncatedKey = "2sxc.Truncated";
    // ponytail: one lock for the bounded diagnostic buffer; partition only if profiling warrants it.
    private readonly object _sync = new();
    private readonly Dictionary<string, Bundle> _logs = new();
    private readonly Dictionary<string, Bundle> _latestBySource = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<Bundle>> _bundlesByLog = new();
    private readonly Dictionary<LogEvent, (long Bytes, int References)> _eventUsage = new(new EventReferenceComparer());
    private readonly Dictionary<string, List<string>> _segments = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _order = new();
    private readonly LinkedList<PendingEvent> _pending = new();
    private long _bytes;
    private long _dropped;
    private long _evicted;

    public string Status
    {
        get
        {
            lock (_sync)
                return $"ILogger store: {_logs.Count}/{MaxLogs} logs, {_pending.Count}/{MaxPendingEntries} pending, ~{_bytes / 1024:N0} KB/{MaxEstimatedBytes / 1024:N0} KB budget; {_dropped} dropped events, {_evicted} evicted logs";
        }
    }

    internal bool Knows(string logId, ImmutableArray<string> ancestors)
    {
        lock (_sync)
            return _logs.ContainsKey(logId) || _latestBySource.ContainsKey(logId)
                || ancestors.Any(id => _logs.ContainsKey(id) || _latestBySource.ContainsKey(id));
    }

    internal void Write(LogEvent data, int segmentSize)
    {
        lock (_sync)
        {
            if (data.Kind == "Admission" && data.Segment != null)
            {
                Admit(data, segmentSize);
                DrainPending(data);
                EnforceBudget();
                return;
            }
            if (data.Kind == "Specs" && data.Segment != null)
            {
                if (_logs.TryGetValue(data.LogId, out var bundle) && bundle.Specs.TryGetValue(data.Segment, out var specs))
                {
                    var merged = Merge(specs, data.Properties);
                    _bytes += Measure(merged) - Measure(specs);
                    bundle.Specs[data.Segment] = merged;
                    EnforceBudget();
                }
                return;
            }

            LogEvent? previousOld = null;
            LogEvent? previousMerged = null;
            Bundle? firstBundle = null;
            HashSet<Bundle>? visited = null;
            // Explicit execution ownership wins over the producer's latest standalone admission.
            // The source is still indexed in every execution bundle for Snapshot(sourceLog).
            var written = data.Ancestors.Length == 0
                && WriteToBundle(data.LogId, data, ref firstBundle, ref visited, ref previousOld, ref previousMerged);
            foreach (var id in data.Ancestors)
                written |= WriteToBundle(id, data, ref firstBundle, ref visited, ref previousOld, ref previousMerged);

            List<string>? missing = null;
            foreach (var id in data.Ancestors)
                if (!_logs.ContainsKey(id) && !_latestBySource.ContainsKey(id)
                    && (missing == null || !missing.Contains(id)))
                    (missing ??= []).Add(id);
            if (missing != null)
                Buffer(data, missing);
            else if (!written && data.Ancestors.Length == 0)
                Buffer(data, [data.LogId]);
            EnforceBudget();
        }
    }

    private bool WriteToBundle(string id, LogEvent data, ref Bundle? firstBundle, ref HashSet<Bundle>? visited,
        ref LogEvent? previousOld, ref LogEvent? previousMerged)
    {
        if (!_logs.TryGetValue(id, out var bundle) && !_latestBySource.TryGetValue(id, out bundle))
            return false;
        if (firstBundle == null)
            firstBundle = bundle;
        else if (ReferenceEquals(firstBundle, bundle))
            return true;
        else if (!(visited ??= [firstBundle]).Add(bundle))
            return true;
        // Merging one bundle's history must not alter the event sent to other bundles.
        var entry = data;
        var exists = bundle.Entries.TryGetValue(data.Sequence, out var old);
        if (!exists && bundle.Entries.Count >= MaxEntriesPerLog)
        {
            bundle.Dropped++;
            _dropped++;
            return true;
        }
        // Replays after late attachment must not erase completed data or exception details.
        if (old?.WrapOpenWasClosed == true && !data.WrapOpenWasClosed)
            return true;
        if (old != null)
        {
            // Ancestor bundles normally share the same old event; share its replacement too.
            entry = ReferenceEquals(old, previousOld) ? previousMerged! : data with
            {
                ExceptionType = data.ExceptionType ?? old.ExceptionType,
                ExceptionText = data.ExceptionText ?? old.ExceptionText,
                ParentOperationId = old.ParentOperationId ?? data.ParentOperationId,
                Depth = old.Depth,
                Properties = Merge(old.Properties, data.Properties),
            };
            previousOld = old;
            previousMerged = entry;
        }
        var size = Measure(entry);
        var delta = size - (old == null ? 0 : Measure(old));
        bundle.Bytes += delta;
        if (old != null)
            Release(old);
        if (_eventUsage.TryGetValue(entry, out var usage))
            _eventUsage[entry] = (usage.Bytes, usage.References + 1);
        else
        {
            _eventUsage[entry] = (size, 1);
            _bytes += size;
        }
        bundle.Entries[data.Sequence] = entry;
        Index(bundle, id, data.LogId);
        foreach (var ancestor in entry.Ancestors)
            Index(bundle, id, ancestor);
        if (entry.Properties.ContainsKey(TruncatedKey) && old?.Properties.ContainsKey(TruncatedKey) != true)
            bundle.Truncated++;
        return true;
    }

    private void Buffer(LogEvent data, IEnumerable<string> ids)
    {
        var pending = new PendingEvent(data, ids.ToHashSet(StringComparer.Ordinal), Measure(data));
        if (pending.Remaining.Count == 0)
            return;
        _pending.AddLast(pending);
        _bytes += pending.Bytes;
    }

    private void DrainPending(LogEvent admission)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal) { admission.LogId };
        if (admission.Properties.TryGetValue(LogExecution.SourceLogIdKey, out var sourceLogId))
            ids.Add(sourceLogId);
        for (var node = _pending.First; node != null;)
        {
            var next = node.Next;
            if (node.Value.Remaining.RemoveWhere(ids.Contains) != 0)
            {
                LogEvent? previousOld = null;
                LogEvent? previousMerged = null;
                Bundle? firstBundle = null;
                HashSet<Bundle>? visited = null;
                WriteToBundle(admission.LogId, node.Value.Data, ref firstBundle, ref visited, ref previousOld, ref previousMerged);
                if (node.Value.Remaining.Count == 0)
                    RemovePending(node, dropped: false);
            }
            node = next;
        }
    }

    private void RemovePending(LinkedListNode<PendingEvent> node, bool dropped)
    {
        _bytes -= node.Value.Bytes;
        _pending.Remove(node);
        if (dropped)
            _dropped++;
    }

    private void Index(Bundle bundle, string bundleId, string logId)
    {
        if (logId == bundleId || !bundle.IndexedLogs.Add(logId))
            return;
        if (!_bundlesByLog.TryGetValue(logId, out var bundles))
            _bundlesByLog[logId] = bundles = [];
        bundles.Add(bundle);
    }

    /// <summary>
    /// Keeps what was captured first, adds what still fits and discloses the rest.
    /// A plain Take would drop already stored identity in unspecified dictionary order.
    /// </summary>
    private static ImmutableDictionary<string, string> Merge(ImmutableDictionary<string, string> stored, ImmutableDictionary<string, string> updates)
    {
        var merged = stored.ToBuilder();
        var truncated = false;
        foreach (var pair in updates)
            if (merged.Count < MaxProperties || merged.ContainsKey(pair.Key))
                merged[pair.Key] = pair.Value;
            else
                truncated = true;
        if (truncated)
            merged[TruncatedKey] = "true";
        return merged.ToImmutable();
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
        if (data.Properties.TryGetValue(LogExecution.SourceLogIdKey, out var sourceLogId))
        {
            bundle.SourceLogId = sourceLogId;
            _latestBySource[sourceLogId] = bundle;
            Index(bundle, data.LogId, sourceLogId);
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
        while (_pending.First != null && (_pending.Count > MaxPendingEntries || _bytes > MaxEstimatedBytes))
            RemovePending(_pending.First, dropped: true);
        while (_order.First != null && (_logs.Count > MaxLogs || _bytes > MaxEstimatedBytes))
        {
            var id = _order.First.Value;
            foreach (var segment in _logs[id].Specs.Keys.ToArray())
                RemoveMembership(segment, id);
            _logs.Remove(id);
            _order.Remove(id);
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
        foreach (var entry in bundle.Entries.Values)
            Release(entry);
        _bytes -= 256;
        foreach (var logId in bundle.IndexedLogs)
        {
            var bundles = _bundlesByLog[logId];
            bundles.Remove(bundle);
            if (bundles.Count == 0)
                _bundlesByLog.Remove(logId);
        }
        if (bundle.SourceLogId is { } sourceLogId
            && _latestBySource.TryGetValue(sourceLogId, out var latest) && latest == bundle)
        {
            var previous = _order.Reverse().Select(logId => _logs[logId])
                .FirstOrDefault(candidate => candidate != bundle && candidate.SourceLogId == sourceLogId);
            if (previous == null)
                _latestBySource.Remove(sourceLogId);
            else
                _latestBySource[sourceLogId] = previous;
        }
        _logs.Remove(id);
        _order.Remove(id);
    }

    private void Release(LogEvent entry)
    {
        var usage = _eventUsage[entry];
        if (usage.References > 1)
            _eventUsage[entry] = (usage.Bytes, usage.References - 1);
        else
        {
            _bytes -= usage.Bytes;
            _eventUsage.Remove(entry);
        }
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
            if (!_bundlesByLog.TryGetValue(logId, out var bundles))
                return null;
            // Only inspect indexed captures; an unknown log never scans the retained store.
            var entries = bundles.SelectMany(b => b.Entries.Values)
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

    // A replay can retain different payloads for the same sequence in different captures.
    private sealed class EventReferenceComparer : IEqualityComparer<LogEvent>
    {
        public bool Equals(LogEvent? x, LogEvent? y) => ReferenceEquals(x, y);
        public int GetHashCode(LogEvent obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class Bundle(string id, DateTime created)
    {
        public string Id { get; } = id;
        public DateTime Created { get; } = created;
        public Dictionary<long, LogEvent> Entries { get; } = new();
        public HashSet<string> IndexedLogs { get; } = [];
        public string? SourceLogId;
        public Dictionary<string, ImmutableDictionary<string, string>> Specs { get; } = new(StringComparer.OrdinalIgnoreCase);
        public long Bytes;
        public int Dropped;
        public int Truncated;
    }

    private sealed record PendingEvent(LogEvent Data, HashSet<string> Remaining, long Bytes);
}
