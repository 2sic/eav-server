namespace ToSic.Sys.Logging;

[CollectionDefinition(nameof(LogEventBridgeTests), DisableParallelization = true)]
public sealed class LogEventBridgeTestCollection;

[Collection(nameof(LogEventBridgeTests))]
public class LogEventBridgeTests
{
    [Fact]
    public void Write_ExportsEntryAndCompletion_InRealTime()
    {
        var sink = new RecordingLogEventSink();
        LogEventBridge.SetSink(sink);
        try
        {
            var log = new Log("Tst.Bridge");

            log.Fn(timer: true).Done("done");

            Equal(2, sink.Events.Count);
            False(sink.Events[0].IsCompletion);
            True(sink.Events[1].IsCompletion);
            Equal("done", sink.Events[1].Result);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Write_IgnoresSinkFailure()
    {
        LogEventBridge.SetSink(new ThrowingLogEventSink());
        try
        {
            var exception = Record.Exception(() => new Log("Tst.Bridge").A("message"));

            Null(exception);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    private sealed class RecordingLogEventSink : ILogEventSink
    {
        public List<(bool IsCompletion, string? Result)> Events { get; } = [];

        public void Write(ILog log, Entry entry) => Events.Add((entry.WrapOpenWasClosed, entry.Result));
    }

    private sealed class ThrowingLogEventSink : ILogEventSink
    {
        public void Write(ILog log, Entry entry) => throw new InvalidOperationException();
    }
}
