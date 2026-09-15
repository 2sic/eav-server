using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

/// <summary>A detached, consistent Insights read. The logger graph is never reconstructed.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record LogSnapshot
{
    public string LogId { get; init; } = "";
    public DateTime Created { get; init; }
    public ImmutableArray<LogEvent> Entries { get; init; } = [];
    public ImmutableDictionary<string, string> Specs { get; init; }
        = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase);
    public string? Title => Specs.TryGetValue(LogStoreEntry.TitleKey, out var title) ? title : null;
    public long EstimatedBytes { get; init; }
    public int DroppedEntries { get; init; }
    public int TruncatedEntries { get; init; }

}

/// <summary>A grouped, detached view of events waiting for log admission.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record PendingLogGroup
{
    public string MissingId { get; init; } = "";
    public string Source { get; init; } = "";
    public int Count { get; init; }
    public DateTime First { get; init; }
    public DateTime Last { get; init; }
    public string? Sample { get; init; }
}
