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

    internal static LogSnapshot FromLegacy(Log log, IDictionary<string, string>? specs = null)
        => new()
        {
            LogId = log.LogId, Created = log.Created,
            Entries = log.SnapshotEntries()
                .Where(e => !e.WrapClose)
                .Select(e => LogEvent.FromEntry(e))
                .ToImmutableArray(),
            Specs = specs?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)
                ?? ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase),
            EstimatedBytes = log.EstimateSize().Total,
        };
}
