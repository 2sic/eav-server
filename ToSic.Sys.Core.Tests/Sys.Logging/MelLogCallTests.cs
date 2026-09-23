using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

public class MelLogCallTests
{
    [Fact]
    public void Done_EmitsOneDebugCompletionWithoutOpeningEntry()
    {
        var (recording, log) = NewLog();
        var call = log.Fn(parameters: "value", message: "opening", cPath: "C:\\full\\source.cs", cName: "Method", cLine: 42)!;

        Null(call.Entry);
        Empty(recording.Entries);

        call.Done("complete");

        var entry = Single(recording.Entries);
        Equal(LogLevel.Debug, entry.Level);
        Equal("Method(value) opening", entry.Value("Operation"));
        Equal("complete", entry.Value("CompletionMessage"));
        Equal("Method(value) opening complete", entry.Value("Message"));
        Equal("C:\\full\\source.cs", entry.Value("SourceFilePath"));
        Equal("Method", entry.Value("SourceMemberName"));
        Equal(42, entry.Value("SourceLineNumber"));
    }

    [Fact]
    public void Return_EmitsDetachedResultAndPreservesValue()
    {
        var (recording, log) = NewLog();
        var call = log.Fn<int>(cName: "Value")!;

        Equal(5, call.Return(5, "done"));

        var entry = Single(recording.Entries);
        Equal("5", entry.Value("Result"));
        Equal(true, entry.Value("HasResult"));
    }

    [Fact]
    public void Return_DoesNotThrowWhenResultTextFails()
    {
        var (_, log) = NewLog();
        var value = new ThrowingText();

        Same(value, log.Fn<ThrowingText>()!.Return(value));
    }

    [Fact]
    public void Done_WithTimerStopsItAndIncludesDuration()
    {
        var (recording, log) = NewLog();
        var call = log.Fn(timer: true)!;

        call.Done();

        False(call.Timer.IsRunning);
        True((long)Single(recording.Entries).Value("DurationMilliseconds")! >= 0);
        True((long)Single(recording.Entries).Value("DurationTicks")! >= 0);
        IsType<DateTime>(Single(recording.Entries).Value("StartedUtc"));
    }

    [Fact]
    public void Done_WithoutTimerHasStartButNoMeasuredDuration()
    {
        var (recording, log) = NewLog();
        var call = log.Fn()!;

        call.Done();

        var entry = Single(recording.Entries);
        IsType<DateTime>(entry.Value("StartedUtc"));
        Null(entry.Value("DurationMilliseconds"));
        Null(entry.Value("DurationTicks"));
    }

    [Fact]
    public void InnerMessageAndRepeatedCompletionUseUnderlyingMelLogOnce()
    {
        var (recording, log) = NewLog();
        var call = log.Fn<int>()!;

        call.A("inner");
        Equal(1, call.Return(1, "first"));
        Equal(2, call.Return(2, "second"));

        Equal([LogLevel.Trace, LogLevel.Debug], recording.Entries.Select(entry => entry.Level));
        Equal("inner", recording.Entries[0].Value("Message"));
        Equal("first", recording.Entries[1].Value("CompletionMessage"));
    }

    [Fact]
    public void LegacyRepeatedCompletionPreservesExistingBehavior()
    {
        var log = new Log("Legacy");
        var call = log.Fn()!;

        call.Done("first");
        call.Done("second");

        Equal(-1, log.WrapDepth);
        Equal(2, log.Entries.Count(entry => entry.WrapClose));
    }

    private static (MelLogTests.RecordingLoggerFactory Recording, ILog Log) NewLog()
    {
        var recording = new MelLogTests.RecordingLoggerFactory(true);
        var factory = new MelLogFactory(recording);
        return (recording, factory.Create("App.Log", null, new CodeRef()));
    }

    private sealed class ThrowingText
    {
        public override string ToString() => throw new InvalidOperationException();
    }
}
