using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

public class InsightsLogSnapshotTests
{
    [Fact]
    public void LegacyReader_DetachesMessagesResultsSpecsSourceAndExceptionText()
    {
        var store = new TestLiveStore();
        var log = new Log("Legacy.Log");
        const string path = "C:\\full\\legacy.cs";
        const string url = "https://example.test/path?a=one&b=two%20words";
        log.A("legacy message", cPath: path, cName: "LegacyMember", cLine: 42, options: new() { HideCodeReference = true, ShowNewLines = true });
        log.Do(() => "legacy result", cPath: path, cName: "LegacyCall", cLine: 43);
        log.Ex(new InvalidOperationException("legacy failure"));
        var entry = store.Add("webapi", log)!;
        entry.AddSpec("Url", url);
        entry.AddSpec("Query", "?a=one&b=two%20words");

        var group = Single(new LegacyInsightsLogSnapshotReader(store).ListGroups());
        var first = group.Events[0];

        Equal("legacy message", first.Message);
        Equal(path, first.SourceFilePath);
        Equal("LegacyMember", first.SourceMemberName);
        Equal(42, first.SourceLineNumber);
        Equal(log.NameId, first.Category);
        Equal(log.FullIdentifier, first.FullSource);
        Equal(log.NameId, first.ShortSource);
        True(first.HideCodeReference);
        True(first.ShowNewLines);
        Contains(group.Events, item => item.Result == "legacy result");
        Equal(url, group.Specs["Url"]);
        Equal("?a=one&b=two%20words", group.Specs["Query"]);
        Contains(group.Events, item => item.Message?.Contains("InvalidOperationException") == true);
    }

    [Fact]
    public void MelReader_DetachesParityValuesAndSupportsControls()
    {
        var store = new InsightsLogStore();
        const string url = "https://example.test/path?a=one&b=two%20words";
        store.Append(new()
        {
            Sequence = 1,
            TimestampUtc = new(2026, 1, 1, 1, 2, 3, DateTimeKind.Utc),
            Category = "ToSic.App",
            Level = InsightsLogLevel.Error,
            EventId = 7,
            EventName = "Failure",
            Message = "mel message",
            SourceFilePath = "C:\\full\\mel.cs",
            SourceMemberName = "MelMember",
            SourceLineNumber = 44,
            Operation = "App.Run()",
            Result = "mel result",
            DurationMilliseconds = 12,
            TraceId = "trace",
            SpanId = "span",
            Segment = "webapi",
            Specs = ImmutableDictionary<string, string?>.Empty.Add("Url", url).Add("Query", "?a=one&b=two%20words"),
            Exception = new("System.InvalidOperationException", "mel failure", "stack", "full exception details", ImmutableDictionary<string, string?>.Empty.Add("Code", "42"))
        });
        var reader = new MelInsightsLogSnapshotReader(store);
        var group = Single(reader.ListGroups());
        var item = Single(reader.ReadGroup(group.Id)!.Events);

        Equal("mel message", item.Message);
        Equal("mel result", item.Result);
        Equal(url, group.Specs["Url"]);
        Equal("C:\\full\\mel.cs", item.SourceFilePath);
        Equal("MelMember", item.SourceMemberName);
        Equal(44, item.SourceLineNumber);
        Equal("full exception details", item.Exception!.Details);
        Equal("42", item.Exception.Data["Code"]);

        reader.Pause();
        True(reader.Snapshot().IsPaused);
        reader.Resume();
        False(reader.Snapshot().IsPaused);
        reader.FlushSegment("webapi");
        Empty(reader.ListGroups());
    }

    [Fact]
    public void LegacyReader_ExposesPauseAndFlushWithoutRetainingStoreObjects()
    {
        var store = new TestLiveStore();
        store.Add("module", new Log("Legacy"));
        var reader = new LegacyInsightsLogSnapshotReader(store);

        reader.Pause();
        True(reader.Snapshot().IsPaused);
        reader.Resume();
        reader.FlushSegment("module");

        Empty(reader.ListGroups());
    }

    private sealed class TestLiveStore : ILogStoreLive
    {
        public int MaxItems => 100;
        public int SegmentSize { get; set; } = 100;
        public ConcurrentDictionary<string, FixedSizedQueue<LogStoreEntry>> Segments { get; } = [];
        public bool Pause { get; set; }
        public int AddCount { get; private set; }

        public LogStoreEntry? Add(string segment, ILog log) => AddEntry(segment, log);
        public LogStoreEntry? ForceAdd(string key, ILog log) => AddEntry(key, log);
        public void FlushSegment(string segment) => Segments.TryRemove(segment, out _);

        private LogStoreEntry AddEntry(string segment, ILog log)
        {
            var entry = new LogStoreEntry { Log = log };
            Segments.GetOrAdd(segment, _ => new(SegmentSize)).Enqueue(entry);
            AddCount++;
            return entry;
        }
    }
}
