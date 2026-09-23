using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record InsightsEvent
{
    /// <summary>Provider order used for stable reads and eviction when events are appended out of order.</summary>
    public long Sequence { get; init; }
    public DateTime TimestampUtc { get; init; }
    /// <summary>MEL logger category identifying the producer; Segment selects its Insights bucket.</summary>
    public string Category { get; init; } = "";
    public InsightsLogLevel Level { get; init; }
    public int EventId { get; init; }
    public string? EventName { get; init; }
    public string? Message { get; init; }
    /// <summary>Detached scalar copy of the MEL structured state, including custom values.</summary>
    public ImmutableDictionary<string, string?> Properties { get; init; } = ImmutableDictionary<string, string?>.Empty;
    /// <summary>Insights metadata such as AppId and ModuleId, kept separate from ordinary state keys.</summary>
    public ImmutableDictionary<string, string?> Specs { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public string? SourceFilePath { get; init; }
    public string? SourceMemberName { get; init; }
    public int? SourceLineNumber { get; init; }
    /// <summary>Name of the completed log call, when this event records a completion.</summary>
    public string? Operation { get; init; }
    /// <summary>Text snapshot of a completed call's result; it can include rendered HTML.</summary>
    public string? Result { get; init; }
    /// <summary>Whole milliseconds for existing consumers; DurationTicks keeps finer timing.</summary>
    public long? DurationMilliseconds { get; init; }
    /// <summary>Start of a completed call, used when calculating the Insights group timestamp.</summary>
    public DateTime? StartedUtc { get; init; }
    /// <summary>Elapsed TimeSpan ticks for precise Insights timing.</summary>
    public long? DurationTicks { get; init; }
    /// <summary>Ambient Activity correlation for request-wide reads; it can change within one log history.</summary>
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    /// <summary>Named Insights segment such as "module", used to filter and flush many log histories.</summary>
    public string? Segment { get; init; }
    /// <summary>ID of one admitted log history, shared by its linked events even when the Activity changes.</summary>
    public string? LogGroupId { get; init; }
    /// <summary>Detached diagnostic details; Insights does not retain the live Exception object.</summary>
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
