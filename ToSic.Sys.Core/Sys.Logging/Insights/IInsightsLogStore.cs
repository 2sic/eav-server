using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IInsightsLogStore
{
    InsightsAppendResult Append(InsightsEvent entry);

    void Pause();
    void Resume();
    void FlushGroup(string traceId);
    void FlushSegment(string? segment);
    void Flush();
    InsightsLogStoreSnapshot Snapshot();
    ImmutableArray<InsightsEvent> ReadGroup(string traceId);
    ImmutableArray<InsightsEvent> List(string? segment = default);
    ImmutableArray<InsightsGroupSummary> ListGroups();
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public enum InsightsAppendResult
{
    Added,
    DroppedPaused,
    DroppedOversized
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogStoreOptions
{
    public int MaxEvents { get; init; } = 4096;
    public long MaxEstimatedBytes { get; init; } = 16 * 1024 * 1024;
    public int MaxGroups { get; init; } = 500;
    public int MaxEventsPerGroup { get; init; } = 4096;
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogStoreCounters(
    long Appended,
    long DroppedPaused,
    long DroppedOversized,
    long EvictedEvents,
    long EvictedGroups,
    int RetainedEvents,
    long RetainedEstimatedBytes,
    int RetainedGroups);

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogStoreSnapshot(
    ImmutableArray<InsightsEvent> Events,
    InsightsLogStoreCounters Counters,
    bool Paused);

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsGroupSummary(string? TraceId, ImmutableArray<string?> Segments, long FirstSequence, int EventCount, bool IsCorrelated);
