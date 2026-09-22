using System.Collections.Immutable;
using System.Text;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class MelInsightsLogSnapshotReader(IInsightsLogStore store) : IInsightsLogSnapshotReader
{
    public InsightsLogSnapshot Snapshot()
    {
        var snapshot = store.Snapshot();
        return new(snapshot.Paused, Groups(snapshot.Events));
    }

    public ImmutableArray<InsightsLogGroupSnapshot> ListGroups() => Snapshot().Groups;

    public InsightsLogGroupSnapshot? ReadGroup(string groupId)
        => ListGroups().FirstOrDefault(group => group.Id == groupId);

    public void Pause() => store.Pause();
    public void Resume() => store.Resume();

    public void FlushGroup(string groupId)
    {
        var group = ReadGroup(groupId);
        if (group == null)
            return;
        if (group.TraceId != null)
            store.FlushGroup(group.TraceId);
        else if (group.Segments.FirstOrDefault() is { } segment)
            store.FlushSegment(segment);
    }

    public void FlushSegment(string segment) => store.FlushSegment(segment);
    public void Flush() => store.Flush();

    // A trace is one request history; without a trace the explicit segment is the group.
    private static ImmutableArray<InsightsLogGroupSnapshot> Groups(IEnumerable<InsightsEvent> entries)
        => entries
            .GroupBy(entry => string.IsNullOrEmpty(entry.TraceId) ? $"segment:{SegmentId(entry.Segment)}" : $"trace:{entry.TraceId}")
            .Select(Group)
            .OrderBy(group => group.Events[0].Sequence)
            .ToImmutableArray();

    private static InsightsLogGroupSnapshot Group(IEnumerable<InsightsEvent> source)
    {
        var retained = source
            .OrderBy(entry => entry.Sequence)
            .ToArray();
        var events = retained
            .Select(Event)
            .ToImmutableArray();
        var specs = ImmutableDictionary.CreateBuilder<string, string?>();
        foreach (var entry in events)
            foreach (var spec in entry.Specs)
                specs[spec.Key] = spec.Value;
        var traceId = retained[0].TraceId;
        var segments = retained.Select(entry => entry.Segment).Distinct().OrderBy(segment => segment, StringComparer.Ordinal).ToImmutableArray();
        return new(
            string.IsNullOrEmpty(traceId) ? $"segment:{SegmentId(segments.FirstOrDefault())}" : $"trace:{traceId}",
            traceId,
            segments,
            events[0].TimestampUtc,
            specs.ToImmutable(),
            events);
    }

    private static InsightsLogEventSnapshot Event(InsightsEvent entry)
        => new(entry.Sequence, entry.TimestampUtc, entry.Category, entry.Level, entry.EventId, entry.EventName, entry.Message,
            entry.Properties, entry.Specs, entry.SourceFilePath, entry.SourceMemberName, entry.SourceLineNumber,
            entry.Operation, entry.Result, entry.DurationMilliseconds, entry.TraceId, entry.SpanId, entry.Segment,
            entry.Exception, null, false, false, false);

    private static string SegmentId(string? segment)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(segment ?? ""));
}
