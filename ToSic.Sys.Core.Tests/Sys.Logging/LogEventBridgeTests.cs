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

    [Fact]
    public void Write_SkipsLiveAndReplay_WhenSinkIsDisabled()
    {
        var sink = new DisabledLogEventSink();
        LogEventBridge.SetSink(sink);
        try
        {
            var log = new Log("Tst.Bridge");

            log.A("message");
            LogEventBridge.Replay(log);

            Empty(sink.Events);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsEnabled_SuppressesReentry_AndRestoresGuardAfterFailure(bool fail)
    {
        var sink = new ReentrantLogEventSink { Fail = fail };
        LogEventBridge.SetSink(sink);
        try
        {
            new Log("Tst.Bridge").A("outer");
            Equal(1, sink.Checks);
            Equal(fail ? 0 : 1, sink.Writes);

            sink.Fail = false;
            new Log("Tst.Bridge").A("after check");
            Equal(2, sink.Checks);
            Equal(fail ? 1 : 2, sink.Writes);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    private sealed class ReentrantLogEventSink : ILogEventSink
    {
        internal bool Fail;
        internal int Checks;
        internal int Writes;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            if (++Checks == 1)
                new Log("Tst.Reentry").A("from IsEnabled");
            if (Fail)
                throw new InvalidOperationException("filter failure");
            return true;
        }

        public void Write(LogEvent entry, Exception? exception = null) => Writes++;
    }

    private sealed class RecordingLogEventSink : ILogEventSink
    {
        public List<(bool IsCompletion, string? Result)> Events { get; } = [];

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Write(LogEvent entry, Exception? exception = null)
            => Events.Add((entry.WrapOpenWasClosed, entry.Result));
    }

    private sealed class DisabledLogEventSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;

        public void Write(LogEvent entry, Exception? exception = null) => Events.Add(entry);
    }

    private sealed class ThrowingLogEventSink : ILogEventSink
    {
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Write(LogEvent entry, Exception? exception = null) => throw new InvalidOperationException();
    }
}
