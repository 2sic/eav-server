using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

public class MelLogTests
{
    [Fact]
    public void Create_UsesFullCategory_AndLogsInitialTrace()
    {
        var (recording, factory) = NewFactory();

        factory.Create("App.FullName", null, new CodeRef("path", "member", 1), "initial");

        var entry = Single(recording.Entries);
        Equal("ToSic.App.FullName", entry.Category);
        Equal("App.FullName", entry.Value("LogName"));
        Equal(LogLevel.Trace, entry.Level);
        Equal("initial", entry.Value("Message"));
    }

    [Fact]
    public void Add_UsesExplicitLevels_AndDetachedDetails()
    {
        var (recording, factory) = NewFactory();
        var log = factory.Create("App.Log", null, new CodeRef());
        var options = new EntryOptions { HideCodeReference = true, ShowNewLines = true };

        log.A("trace", cPath: "C:\\full\\source.cs", cName: "Member", cLine: 42, options: options);
        log.W("warning");
        log.E("error");

        Equal([LogLevel.Trace, LogLevel.Warning, LogLevel.Error], recording.Entries.Select(entry => entry.Level));
        var trace = recording.Entries[0];
        Equal("C:\\full\\source.cs", trace.Value("SourceFilePath"));
        Equal("Member", trace.Value("SourceMemberName"));
        Equal(42, trace.Value("SourceLineNumber"));
        Equal(true, trace.Value("HideCodeReference"));
        Equal(true, trace.Value("ShowNewLines"));
        Equal("{Message}", trace.Value("{OriginalFormat}"));
    }

    [Fact]
    public void Ex_UsesOneErrorWithSameException()
    {
        var (recording, factory) = NewFactory();
        var log = factory.Create("App.Log", null, new CodeRef());
        var exception = new InvalidOperationException("broken");

        Same(exception, log.Ex("details", exception));

        var entry = Single(recording.Entries);
        Equal(LogLevel.Error, entry.Level);
        Same(exception, entry.Exception);
        Equal("details", entry.Value("Message"));
    }

    [Fact]
    public void Ex_UsesOneErrorForExceptionOnly()
    {
        var (recording, factory) = NewFactory();
        var log = factory.Create("App.Log", null, new CodeRef());
        var exception = new InvalidOperationException("broken");

        Same(exception, log.Ex(exception));

        var entry = Single(recording.Entries);
        Equal(LogLevel.Error, entry.Level);
        Same(exception, entry.Exception);
    }

    [Fact]
    public void Add_SnapshotsAmbientTraceContext()
    {
        var (recording, factory) = NewFactory();
        var log = factory.Create("App.Log", null, new CodeRef());
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();

        log.A("trace");

        var entry = Single(recording.Entries);
        Equal(activity.TraceId.ToString(), entry.Value("TraceId"));
        Equal(activity.SpanId.ToString(), entry.Value("SpanId"));
    }

    [Fact]
    public void Create_UsesParentsMelFactory()
    {
        var (_, factory) = NewFactory();
        var parent = factory.Create("Parent", null, new CodeRef());

        var child = factory.Create("Child", parent, new CodeRef());

        Same(factory, IsType<MelLog>(child).Factory);
    }

    [Fact]
    public void Add_DoesNothingWhenTraceDisabled()
    {
        var (recording, factory) = NewFactory(traceEnabled: false);
        var log = factory.Create("App.Log", null, new CodeRef());

        log.A("trace");

        Empty(recording.Entries);
    }

    private static (RecordingLoggerFactory Recording, MelLogFactory Factory) NewFactory(bool traceEnabled = true)
    {
        var loggerFactory = new RecordingLoggerFactory(traceEnabled);
        return (loggerFactory, new(loggerFactory));
    }

    internal sealed class RecordingLoggerFactory(bool traceEnabled) : ILoggerFactory
    {
        public List<RecordedEvent> Entries { get; } = [];

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, traceEnabled, Entries);

        public void Dispose() { }
    }

    private sealed class RecordingLogger(string category, bool traceEnabled, List<RecordedEvent> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.Trace || traceEnabled;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => entries.Add(new(category, logLevel, (IReadOnlyList<KeyValuePair<string, object?>>)state!, exception));
    }

    internal sealed record RecordedEvent(string Category, LogLevel Level, IReadOnlyList<KeyValuePair<string, object?>> State, Exception? Exception)
    {
        public object? Value(string key) => State.Single(pair => pair.Key == key).Value;
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
