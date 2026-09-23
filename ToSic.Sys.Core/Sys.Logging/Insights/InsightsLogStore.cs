using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class InsightsLogStore : IInsightsLogStore
{
    private sealed class Group(string key)
    {
        public string Key { get; } = key;
        public List<InsightsEvent> Events { get; } = [];
    }

    // One lock keeps the retained events, limits and counters consistent with each snapshot.
    private readonly object _sync = new();
    private readonly InsightsLogStoreOptions _options;
    private readonly Dictionary<string, Group> _groups = [];
    private readonly Dictionary<string, List<InsightsEvent>> _unscopedBySegment = [];
    private long _estimatedBytes;
    private int _eventCount;
    private long _appended;
    private long _droppedPaused;
    private long _droppedOversized;
    private long _evictedEvents;
    private long _evictedGroups;
    private bool _paused;

    public InsightsLogStore(InsightsLogStoreOptions? options = default)
    {
        _options = options ?? new();
        if (_options.MaxEvents < 1 || _options.MaxEstimatedBytes < 1 || _options.MaxGroups < 1 || _options.MaxEventsPerGroup < 1)
            throw new ArgumentOutOfRangeException(nameof(options));
    }

    public InsightsAppendResult Append(InsightsEvent entry)
    {
        if (entry.Sequence <= 0)
            throw new ArgumentOutOfRangeException(nameof(entry));
        var estimatedBytes = Estimate(entry);
        lock (_sync)
        {
            if (_paused)
            {
                _droppedPaused++;
                return InsightsAppendResult.DroppedPaused;
            }
            if (estimatedBytes > _options.MaxEstimatedBytes)
            {
                _droppedOversized++;
                return InsightsAppendResult.DroppedOversized;
            }

            var stored = entry with
            {
                TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc.ToUniversalTime()
            };
            Add(stored, estimatedBytes);
            EnforceLimits();
            _appended++;
            return InsightsAppendResult.Added;
        }
    }

    public void Pause()
    {
        lock (_sync)
            _paused = true;
    }

    public void Resume()
    {
        lock (_sync)
            _paused = false;
    }

    public void FlushGroup(string traceId)
    {
        // A trace lookup removes each matching log history, including events written under another Activity.
        lock (_sync)
        {
            foreach (var group in _groups.Values.Where(group => group.Key == traceId || group.Events.Any(entry => entry.TraceId == traceId)).ToList())
            {
                _groups.Remove(group.Key);
                Remove(group.Events);
            }
        }
    }

    public void FlushSegment(string? segment)
    {
        var key = segment ?? "";
        lock (_sync)
        {
            if (_unscopedBySegment.TryGetValue(key, out var events))
            {
                _unscopedBySegment.Remove(key);
                Remove(events);
            }
            foreach (var group in _groups.Values.ToList())
            {
                var matching = group.Events.Where(entry => entry.Segment == segment).ToList();
                foreach (var entry in matching)
                {
                    group.Events.Remove(entry);
                    Remove([entry]);
                }
                if (group.Events.Count == 0)
                    _groups.Remove(group.Key);
            }
        }
    }

    public void Flush()
    {
        lock (_sync)
        {
            _groups.Clear();
            _unscopedBySegment.Clear();
            _eventCount = 0;
            _estimatedBytes = 0;
        }
    }

    public InsightsLogStoreSnapshot Snapshot()
    {
        lock (_sync)
            return new(AllEvents().OrderBy(eventInfo => eventInfo.Sequence).ToImmutableArray(), Counters(), _paused);
    }

    public ImmutableArray<InsightsEvent> ReadGroup(string traceId)
    {
        // Return matching log histories as a detached ordered view, even if their Activity changed.
        lock (_sync)
            return _groups.Values
                .Where(group => group.Key == traceId || group.Events.Any(entry => entry.TraceId == traceId))
                .SelectMany(group => group.Events)
                .OrderBy(eventInfo => eventInfo.Sequence)
                .ToImmutableArray();
    }

    public ImmutableArray<InsightsEvent> List(string? segment = default)
    {
        lock (_sync)
            return AllEvents()
                .Where(eventInfo => segment == null || eventInfo.Segment == segment)
                .OrderBy(eventInfo => eventInfo.Sequence)
                .ToImmutableArray();
    }

    public ImmutableArray<InsightsGroupSummary> ListGroups()
    {
        // The group key may now be a log ID; expose an event TraceId for request correlation.
        lock (_sync)
            return _groups.Values
                .Select(group => new InsightsGroupSummary(group.Events.Select(entry => entry.TraceId).FirstOrDefault(traceId => traceId != null), group.Events.Select(eventInfo => eventInfo.Segment).Distinct().OrderBy(segment => segment, StringComparer.Ordinal).ToImmutableArray(), group.Events.Min(eventInfo => eventInfo.Sequence), group.Events.Count, true))
                .Concat(_unscopedBySegment.Select(pair => new InsightsGroupSummary(null, ImmutableArray.Create<string?>(pair.Key == "" ? null : pair.Key), pair.Value.Min(eventInfo => eventInfo.Sequence), pair.Value.Count, false)))
                .OrderBy(group => group.FirstSequence)
                .ToImmutableArray();
    }

    private void Add(InsightsEvent entry, long estimatedBytes)
    {
        // Admitted logs keep their own history; Activity still correlates them across a request.
        var groupKey = !string.IsNullOrEmpty(entry.LogGroupId) ? $"log:{entry.LogGroupId}" : entry.TraceId;
        if (!string.IsNullOrEmpty(groupKey))
        {
            if (!_groups.TryGetValue(groupKey, out var group))
                _groups[groupKey] = group = new(groupKey);
            group.Events.Add(entry);
            while (group.Events.Count > _options.MaxEventsPerGroup)
                Remove(group.Events, Oldest(group.Events));
        }
        else
        {
            var segment = entry.Segment ?? "";
            if (!_unscopedBySegment.TryGetValue(segment, out var events))
                _unscopedBySegment[segment] = events = [];
            events.Add(entry);
            while (events.Count > _options.MaxEventsPerGroup)
                Remove(events, Oldest(events));
        }
        _eventCount++;
        _estimatedBytes += estimatedBytes;
    }

    private void EnforceLimits()
    {
        // Evict the oldest complete container so Insights does not keep half of an older request.
        while (GroupCount > _options.MaxGroups)
            EvictOldestContainer();
        while (_eventCount > _options.MaxEvents || _estimatedBytes > _options.MaxEstimatedBytes)
            EvictOldestContainer();
    }

    private void EvictGroup(Group group)
    {
        _groups.Remove(group.Key);
        _evictedEvents += group.Events.Count;
        Remove(group.Events);
        _evictedGroups++;
    }

    private void EvictOldestContainer()
    {
        // Keep the process boot history inspectable while ordinary histories turn over.
        var oldestGroup = _groups.Values
            .Where(group => !IsBootLog(group))
            .OrderBy(group => Oldest(group.Events).Sequence)
            .FirstOrDefault();
        var oldestUnscoped = _unscopedBySegment
            .Where(pair => pair.Value.Count > 0)
            .OrderBy(pair => Oldest(pair.Value).Sequence)
            .FirstOrDefault();
        if (oldestGroup == null && oldestUnscoped.Value == null)
            oldestGroup = _groups.Values.OrderBy(group => Oldest(group.Events).Sequence).FirstOrDefault();
        if (oldestGroup != null && (oldestUnscoped.Value == null || Oldest(oldestGroup.Events).Sequence <= Oldest(oldestUnscoped.Value).Sequence))
            EvictGroup(oldestGroup);
        else
            EvictOldestUnscoped();
    }

    private static bool IsBootLog(Group group)
        => group.Events.Any(entry => entry.Segment == "boot-log" && entry.Category == "ToSic.Sys.BootLog");

    private bool EvictOldestUnscoped()
    {
        var pair = _unscopedBySegment
            .Where(pair => pair.Value.Count > 0)
            .OrderBy(pair => Oldest(pair.Value).Sequence)
            .FirstOrDefault();
        if (pair.Value == null)
            return false;
        Remove(pair.Value, Oldest(pair.Value));
        if (pair.Value.Count == 0)
            _unscopedBySegment.Remove(pair.Key);
        return true;
    }

    private void Remove(List<InsightsEvent> events, InsightsEvent entry)
    {
        events.Remove(entry);
        Remove([entry]);
        _evictedEvents++;
    }

    private void Remove(IEnumerable<InsightsEvent> events)
    {
        foreach (var entry in events)
        {
            _eventCount--;
            _estimatedBytes -= Estimate(entry);
        }
    }

    private IEnumerable<InsightsEvent> AllEvents()
        => _groups.Values.SelectMany(group => group.Events).Concat(_unscopedBySegment.Values.SelectMany(events => events));

    private static InsightsEvent Oldest(IEnumerable<InsightsEvent> events)
        => events.OrderBy(entry => entry.Sequence).First();

    private InsightsLogStoreCounters Counters()
        => new(_appended, _droppedPaused, _droppedOversized, _evictedEvents, _evictedGroups, _eventCount, _estimatedBytes, GroupCount);

    private int GroupCount => _groups.Count + _unscopedBySegment.Count;

    // This is a stable retention budget, not a managed-heap measurement.
    private static long Estimate(InsightsEvent entry)
        => 144
           + Length(entry.Category) + Length(entry.EventName) + Length(entry.Message)
           + Length(entry.SourceFilePath) + Length(entry.SourceMemberName) + Length(entry.Operation) + Length(entry.Result)
           + Length(entry.TraceId) + Length(entry.SpanId) + Length(entry.Segment) + Length(entry.LogGroupId)
           + Estimate(entry.Exception)
           + entry.Properties.Sum(pair => Length(pair.Key) + Length(pair.Value))
           + entry.Specs.Sum(pair => Length(pair.Key) + Length(pair.Value));

    private static int Length(string? value) => value?.Length ?? 0;

    private static long Estimate(InsightsExceptionDiagnostic? exception)
        => exception == null
            ? 0
            : Length(exception.Type) + Length(exception.Message) + Length(exception.StackTrace) + Length(exception.Details)
              + exception.Data.Sum(pair => Length(pair.Key) + Length(pair.Value))
              + Estimate(exception.InnerException);
}
