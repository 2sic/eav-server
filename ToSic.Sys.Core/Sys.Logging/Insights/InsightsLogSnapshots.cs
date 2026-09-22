using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogSnapshot(bool IsPaused, ImmutableArray<InsightsLogGroupSnapshot> Groups);

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogGroupSnapshot(
    string Id,
    string? TraceId,
    ImmutableArray<string?> Segments,
    DateTime TimestampUtc,
    ImmutableDictionary<string, string?> Specs,
    ImmutableArray<InsightsLogEventSnapshot> Events)
{
    private const string TitleKey = " Title";

    public string? Title => Specs.TryGetValue(TitleKey, out var title)
        ? title
        : Events.Length > 0 ? Events[0].Message : null;
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsLogEventSnapshot(
    long Sequence,
    DateTime TimestampUtc,
    string Category,
    InsightsLogLevel Level,
    int EventId,
    string? EventName,
    string? Message,
    ImmutableDictionary<string, string?> Properties,
    ImmutableDictionary<string, string?> Specs,
    string? SourceFilePath,
    string? SourceMemberName,
    int? SourceLineNumber,
    string? Operation,
    string? Result,
    long? DurationMilliseconds,
    string? TraceId,
    string? SpanId,
    string? Segment,
    InsightsExceptionDiagnostic? Exception,
    int? Depth,
    bool WrapOpen,
    bool WrapClose,
    bool WrapOpenWasClosed);
