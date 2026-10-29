using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

public class InsightsLogStoreTests
{
    [Fact]
    public void Append_OrdersImmutableSnapshots()
    {
        var store = new InsightsLogStore();

        store.Append(NewEvent("first", 1, timestamp: new(2026, 1, 1, 1, 0, 0, DateTimeKind.Local)));
        store.Append(NewEvent("second", 2));

        var snapshot = store.Snapshot();
        Equal([1L, 2L], snapshot.Events.Select(entry => entry.Sequence));
        Equal(["first", "second"], snapshot.Events.Select(entry => entry.Message));
        Equal(DateTimeKind.Utc, snapshot.Events[0].TimestampUtc.Kind);
        IsType<ImmutableDictionary<string, string?>>(snapshot.Events[0].Properties);
        Equal("full details", snapshot.Events[0].Exception!.Details);
    }

    [Fact]
    public void Append_EnforcesTotalEventLimitForUnscopedEvents()
    {
        var store = new InsightsLogStore(new() { MaxEvents = 2 });

        store.Append(NewEvent("one", 1, segment: "a"));
        store.Append(NewEvent("two", 2, segment: "b"));
        store.Append(NewEvent("three", 3, segment: "a"));

        Equal(["two", "three"], store.List().Select(entry => entry.Message));
        Equal(1, store.Snapshot().Counters.EvictedEvents);
    }

    [Fact]
    public void Append_EnforcesEstimatedByteLimit()
    {
        var store = new InsightsLogStore(new() { MaxEstimatedBytes = 350 });

        store.Append(NewEvent(new string('a', 100), 1));
        store.Append(NewEvent(new string('b', 100), 2));

        var snapshot = store.Snapshot();
        Single(snapshot.Events);
        True(snapshot.Counters.RetainedEstimatedBytes <= 350);
    }

    [Fact]
    public void Append_EnforcesGroupCountByEvictingOldestGroup()
    {
        var store = new InsightsLogStore(new() { MaxGroups = 1 });

        store.Append(NewEvent("first", 1, traceId: "trace-1"));
        store.Append(NewEvent("first-detail", 2, traceId: "trace-1"));
        store.Append(NewEvent("second", 3, traceId: "trace-2"));

        var snapshot = store.Snapshot();
        Equal(["second"], snapshot.Events.Select(entry => entry.Message));
        Equal(1, snapshot.Counters.EvictedGroups);
        Equal(2, snapshot.Counters.EvictedEvents);
        Equal("trace-2", Single(store.ListGroups()).TraceId);
    }

    [Fact]
    public void Append_EnforcesEventsPerCorrelatedGroup()
    {
        var store = new InsightsLogStore(new() { MaxEventsPerGroup = 2 });

        store.Append(NewEvent("one", 1, traceId: "trace"));
        store.Append(NewEvent("two", 2, traceId: "trace"));
        store.Append(NewEvent("three", 3, traceId: "trace"));

        Equal(["two", "three"], store.List().Select(entry => entry.Message));
    }

    [Fact]
    public void Append_RetainsProcessBootGroup_WhenOtherGroupsExceedLimits()
    {
        var store = new InsightsLogStore(new() { MaxEvents = 2, MaxGroups = 1 });
        var boot = NewEvent("Starting Boot Log", 1, segment: "boot-log") with
        {
            Category = "ToSic.Sys.BootLog",
            LogGroupId = "process-boot"
        };

        store.Append(boot);
        store.Append(NewEvent("first request", 2, traceId: "request-1"));
        store.Append(NewEvent("second request", 3, traceId: "request-2"));

        Equal(["Starting Boot Log"], store.List().Select(entry => entry.Message));
        Equal(1, store.Snapshot().Counters.RetainedGroups);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Append_EvictsLowestSequencePerGroup_WhenArrivalIsOutOfOrder(bool correlated)
    {
        var store = new InsightsLogStore(new() { MaxEventsPerGroup = 2 });
        var traceId = correlated ? "trace" : null;

        store.Append(NewEvent("two", 2, traceId, "segment"));
        store.Append(NewEvent("one", 1, traceId, "segment"));
        store.Append(NewEvent("three", 3, traceId, "segment"));

        Equal([2L, 3L], store.List().Select(entry => entry.Sequence));
    }

    [Fact]
    public void Append_EvictsLowestSequenceForTotalCountByteAndGroupLimits_WhenArrivalIsOutOfOrder()
    {
        var stores = new[]
        {
            new InsightsLogStore(new() { MaxEvents = 4 }),
            new InsightsLogStore(new() { MaxEstimatedBytes = 1400 }),
            new InsightsLogStore(new() { MaxGroups = 2 })
        };

        foreach (var store in stores)
        {
            store.Append(NewEvent(new string('b', 100), 2, traceId: "two"));
            store.Append(NewEvent(new string('d', 100), 4, traceId: "two"));
            store.Append(NewEvent(new string('c', 100), 3, traceId: "three"));
            store.Append(NewEvent(new string('a', 100), 1, traceId: "three"));
            store.Append(NewEvent(new string('e', 100), 5, traceId: "five"));

            Equal([2L, 4L, 5L], store.List().Select(entry => entry.Sequence));
        }
    }

    [Fact]
    public void Append_EvictsLowestSequenceAcrossUnscopedStreams_WhenTotalLimitIsExceeded()
    {
        var store = new InsightsLogStore(new() { MaxEvents = 4 });

        store.Append(NewEvent("two", 2, segment: "two"));
        store.Append(NewEvent("four", 4, segment: "two"));
        store.Append(NewEvent("three", 3, segment: "three"));
        store.Append(NewEvent("one", 1, segment: "three"));
        store.Append(NewEvent("five", 5, traceId: "five"));

        Equal([2L, 3L, 4L, 5L], store.List().Select(entry => entry.Sequence));
    }

    [Fact]
    public void ReadGroup_OrdersEventsAndListsEverySegment()
    {
        var store = new InsightsLogStore();
        store.Append(NewEvent("web", 2, traceId: "trace", segment: "web"));
        store.Append(NewEvent("api", 1, traceId: "trace", segment: "api"));

        Equal(["api", "web"], store.ReadGroup("trace").Select(entry => entry.Message));
        Equal(["api", "web"], Single(store.ListGroups()).Segments.ToArray());
    }

    [Fact]
    public void Append_DropsOversizedAndPausedEventsWithCounters()
    {
        var store = new InsightsLogStore(new() { MaxEstimatedBytes = 200 });

        Equal(InsightsAppendResult.DroppedOversized, store.Append(NewEvent(new string('x', 500), 1)));
        store.Pause();
        Equal(InsightsAppendResult.DroppedPaused, store.Append(NewEvent("paused", 2)));
        store.Resume();

        var counters = store.Snapshot().Counters;
        Equal(1, counters.DroppedOversized);
        Equal(1, counters.DroppedPaused);
        Empty(store.List());
    }

    [Fact]
    public void Flush_RemovesCorrelatedAndUnscopedEvents()
    {
        var store = new InsightsLogStore();
        store.Append(NewEvent("correlated", 1, traceId: "trace", segment: "web"));
        store.Append(NewEvent("unscoped", 2, segment: "web"));
        store.Append(NewEvent("other", 3, segment: "other"));

        store.FlushGroup("trace");
        store.FlushSegment("web");

        Equal(["other"], store.List().Select(entry => entry.Message));
        store.Flush();
        Empty(store.List());
    }

    [Fact]
    public void AppendAndRead_AreThreadSafe()
    {
        var store = new InsightsLogStore(new() { MaxEvents = 100 });
        var failures = new ConcurrentQueue<Exception>();

        Parallel.For(0, 200, index =>
        {
            try
            {
                store.Append(NewEvent(index.ToString(), index + 1, segment: index % 2 == 0 ? "a" : "b"));
                var entries = store.List();
                Equal(entries.OrderBy(entry => entry.Sequence), entries);
            }
            catch (Exception exception)
            {
                failures.Enqueue(exception);
            }
        });

        Empty(failures);
        True(store.Snapshot().Events.Length <= 100);
    }

    [Fact]
    public void Append_BoundsUnscopedSegmentStreamsAndRequiresProviderSequence()
    {
        var store = new InsightsLogStore(new() { MaxGroups = 1 });

        Throws<ArgumentOutOfRangeException>(() => store.Append(NewEvent("missing", 0)));
        store.Append(NewEvent("first", 1, segment: "first"));
        store.Append(NewEvent("second", 2, segment: "second"));

        Equal(["second"], store.List().Select(entry => entry.Message));
        Equal(1, store.Snapshot().Counters.RetainedGroups);
    }

    private static InsightsEvent NewEvent(string message, long sequence, string? traceId = default, string? segment = default, DateTime timestamp = default)
        => new()
        {
            Sequence = sequence,
            TimestampUtc = timestamp,
            Category = "ToSic.Test",
            Level = InsightsLogLevel.Information,
            EventId = 7,
            EventName = "Test",
            Message = message,
            Properties = ImmutableDictionary<string, string?>.Empty.Add("Property", "value"),
            Specs = ImmutableDictionary<string, string?>.Empty.Add("Spec", "value"),
            TraceId = traceId,
            Segment = segment,
            Exception = new("System.InvalidOperationException", "details", "stack", "full details", ImmutableDictionary<string, string?>.Empty.Add("Code", "42"))
        };
}
