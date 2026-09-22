using System.Collections.Immutable;
using System.Text;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
// This is a read-time adapter only; MEL never converts events back into Log or Entry.
public sealed class LegacyInsightsLogSnapshotReader(ILogStoreLive store) : IInsightsLogSnapshotReader
{
    public InsightsLogSnapshot Snapshot() => new(store.Pause, ListGroups());

    public ImmutableArray<InsightsLogGroupSnapshot> ListGroups()
        => store.Segments
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SelectMany(pair => pair.Value.ToArray().Select((entry, index) => Snapshot(pair.Key, index, entry)))
            .ToImmutableArray();

    public InsightsLogGroupSnapshot? ReadGroup(string groupId)
        => ListGroups().FirstOrDefault(group => group.Id == groupId);

    public void Pause() => store.Pause = true;
    public void Resume() => store.Pause = false;

    public void FlushSegment(string segment) => store.FlushSegment(segment);

    public void Flush()
    {
        foreach (var segment in store.Segments.Keys.ToArray())
            store.FlushSegment(segment);
    }

    private static InsightsLogGroupSnapshot Snapshot(string segment, int index, LogStoreEntry entry)
    {
        var specs = (entry.Specs ?? new Dictionary<string, string>())
            .ToImmutableDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.InvariantCultureIgnoreCase);
        var log = entry.Log as Log;
        var events = log == null
            ? ImmutableArray<InsightsLogEventSnapshot>.Empty
            : Entries(log, segment);
        return new(LegacyId(segment, index), null, [segment], (log?.Created ?? DateTime.UtcNow).ToUniversalTime(), specs, events);
    }

    private static ImmutableArray<InsightsLogEventSnapshot> Entries(Log log, string segment)
    {
        // Copy under the Legacy lock, then build detached records outside it.
        Entry[] entries;
        lock (log.Entries)
            entries = log.Entries.ToArray();
        return entries.Select((entry, index) => new InsightsLogEventSnapshot(
                index + 1,
                entry.Created.ToUniversalTime(),
                entry.ShortSource,
                InsightsLogLevel.Trace,
                0,
                null,
                entry.Message,
                ImmutableDictionary<string, string?>.Empty,
                ImmutableDictionary<string, string?>.Empty,
                entry.Code?.Path,
                entry.Code?.Name,
                entry.Code?.Line,
                null,
                entry.Result,
                entry.Elapsed == TimeSpan.Zero ? null : (long)entry.Elapsed.TotalMilliseconds,
                null,
                null,
                segment,
                null,
                entry.Depth,
                entry.WrapOpen,
                entry.WrapClose,
                entry.WrapOpenWasClosed,
                entry.Source,
                entry.ShortSource,
                entry.Options?.HideCodeReference ?? false,
                entry.Options?.ShowNewLines ?? false))
            .ToImmutableArray();
    }

    private static string LegacyId(string segment, int index)
        => $"legacy:{Convert.ToBase64String(Encoding.UTF8.GetBytes(segment))}:{index}";
}
