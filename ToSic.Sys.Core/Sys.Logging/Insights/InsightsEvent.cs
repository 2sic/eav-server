using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsEvent
{
    public long Sequence { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string Category { get; init; } = "";
    public InsightsLogLevel Level { get; init; }
    public int EventId { get; init; }
    public string? EventName { get; init; }
    public string? Message { get; init; }
    public ImmutableDictionary<string, string?> Properties { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public ImmutableDictionary<string, string?> Specs { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public string? SourceFilePath { get; init; }
    public string? SourceMemberName { get; init; }
    public int? SourceLineNumber { get; init; }
    public string? Operation { get; init; }
    public string? Result { get; init; }
    public long? DurationMilliseconds { get; init; }
    public DateTime? StartedUtc { get; init; }
    public long? DurationTicks { get; init; }
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    public string? Segment { get; init; }
    public InsightsExceptionDiagnostic? Exception { get; init; }
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public enum InsightsLogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsExceptionDiagnostic(
    string Type,
    string? Message,
    string? StackTrace,
    string? Details,
    ImmutableDictionary<string, string?> Data,
    InsightsExceptionDiagnostic? InnerException = default);
