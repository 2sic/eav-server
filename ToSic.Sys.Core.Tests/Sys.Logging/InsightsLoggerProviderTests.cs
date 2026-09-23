using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

public class InsightsLoggerProviderTests
{
    [Fact]
    public void Provider_CapturesAcceptedEventWithAllDetachedDetails()
    {
        var store = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(store);
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();
        var exception = new InvalidOperationException("outer", new ArgumentException("inner"));
        exception.Data["Code"] = 42;
        var startedUtc = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

        provider.CreateLogger("ToSic.App").Log(LogLevel.Error, new(42, "Failure"), State(
            ("Message", "state message"),
            ("SourceFilePath", "C:\\full\\source.cs"),
            ("SourceMemberName", "Run"),
            ("SourceLineNumber", 7),
            ("Operation", "App.Run()"),
            ("Result", "failed"),
            ("DurationMilliseconds", 12L),
            ("StartedUtc", startedUtc),
            ("DurationTicks", 123456L),
            ("Segment", "webapi"),
            ("Custom", "value")), exception, static (_, _) => "rendered message");
        exception.Data["Code"] = "changed";

        var entry = Single(store.List());
        Equal(1, entry.Sequence);
        Equal(InsightsLogLevel.Error, entry.Level);
        Equal(42, entry.EventId);
        Equal("Failure", entry.EventName);
        Equal("ToSic.App", entry.Category);
        Equal("rendered message", entry.Message);
        Equal("value", entry.Properties["Custom"]);
        Equal("C:\\full\\source.cs", entry.SourceFilePath);
        Equal("Run", entry.SourceMemberName);
        Equal(7, entry.SourceLineNumber);
        Equal("App.Run()", entry.Operation);
        Equal("failed", entry.Result);
        Equal(12, entry.DurationMilliseconds);
        Equal(startedUtc, entry.StartedUtc);
        Equal(123456L, entry.DurationTicks);
        Equal("webapi", entry.Segment);
        Equal(activity.TraceId.ToString(), entry.TraceId);
        Equal(activity.SpanId.ToString(), entry.SpanId);
        Equal("42", entry.Exception!.Data["Code"]);
        Equal(exception.ToString(), entry.Exception.Details);
        Equal(typeof(ArgumentException).FullName, entry.Exception.InnerException!.Type);
    }

    [Fact]
    public void Provider_RejectsOtherCategories_AndSequencesAcceptedEventsOnce()
    {
        var store = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(store);

        provider.CreateLogger("Microsoft.Hosting").LogTrace("ignored");
        provider.CreateLogger("2sxc.Module").LogWarning("first");
        provider.CreateLogger("ToSic.App").LogInformation("second");

        Equal([1L, 2L], store.List().Select(entry => entry.Sequence));
        Equal([InsightsLogLevel.Warning, InsightsLogLevel.Information], store.List().Select(entry => entry.Level));
    }

    [Fact]
    public async Task Provider_RetainsNewestSequence_WhenDelayedFormatterAppendsOutOfOrder()
    {
        var store = new InsightsLogStore(new() { MaxEventsPerGroup = 1 });
        var provider = new InsightsLoggerProvider(store);
        var logger = provider.CreateLogger("ToSic.App");
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();

        var delayed = Task.Run(() => logger.Log(LogLevel.Information, default, State(("TraceId", "trace")), null, (_, _) =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("The delayed formatter was not released.");
            return "one";
        }));
        try
        {
            True(entered.Wait(TimeSpan.FromSeconds(5)));
            logger.Log(LogLevel.Information, default, State(("TraceId", "trace")), null, static (_, _) => "two");
        }
        finally
        {
            release.Set();
            await delayed;
        }

        Equal([2L], store.List().Select(entry => entry.Sequence));
    }

    [Fact]
    public void Provider_CapturesMetadataSpecsWithoutStandardFieldCollisions()
    {
        var store = new InsightsLogStore();
        var insights = new InsightsLoggerProvider(store);
        using var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(insights);
        });
        var log = new MelLogFactory(factory).Create("App.Log", null, new CodeRef());
        var entry = new MelLogStore().Add("webapi", log)!;
        const string url = "https://example.test/path?a=one&b=two%20words";
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();

        entry.AddSpec("Message", "spec message");
        entry.AddSpec("Specs", "nested collision");
        entry.AddSpec("Url", url);

        var metadata = store.List().Last();
        Equal("Log specs", metadata.Message);
        Equal("Log specs", metadata.Properties["Message"]);
        Equal("spec message", metadata.Specs["Message"]);
        Equal("nested collision", metadata.Specs["Specs"]);
        Equal(url, metadata.Specs["Url"]);
        Equal("webapi", metadata.Segment);
        Equal(activity.TraceId.ToString(), metadata.TraceId);
        Equal(3, store.ReadGroup(activity.TraceId.ToString()).Length);
    }

    [Fact]
    public void Provider_IsolatesStoreFailures()
    {
        var provider = new InsightsLoggerProvider(new ThrowingStore());

        var exception = Record.Exception(() => provider.CreateLogger("ToSic.App").LogInformation("safe"));

        Null(exception);
    }

    [Fact]
    public void Registration_UsesSameProviderAndKeepsTraceLocal()
    {
        var other = new RecordingProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddSysCoreInsightsLogger();
            logging.AddSysCoreInsightsLogger();
            logging.AddProvider(other);
        });
        using var servicesProvider = services.BuildServiceProvider();
        var insights = servicesProvider.GetRequiredService<InsightsLoggerProvider>();
        Same(insights, Single(servicesProvider.GetServices<ILoggerProvider>().Where(provider => provider is InsightsLoggerProvider)));
        var logger = servicesProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ToSic.App");

        logger.LogTrace("trace");
        logger.LogWarning("warning");

        Equal([InsightsLogLevel.Trace, InsightsLogLevel.Warning], servicesProvider.GetRequiredService<IInsightsLogStore>().List().Select(entry => entry.Level));
        Single(other.Entries);
        Equal(LogLevel.Warning, Single(other.Entries).Level);
    }

    [Fact]
    public void MelInsightsRegistration_KeepsOtherProviderIndependent()
    {
        var other = new RecordingProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddProvider(other);
        });
        services.AddSysCoreLogging(useMel: true);

        using var serviceProvider = services.BuildServiceProvider();
        var insights = serviceProvider.GetRequiredService<InsightsLoggerProvider>();
        Same(insights, Single(serviceProvider.GetServices<ILoggerProvider>().Where(provider => provider is InsightsLoggerProvider)));
        IsType<MelLogFactory>(serviceProvider.GetRequiredService<ILogFactory>());
        IsType<MelLogStore>(serviceProvider.GetRequiredService<ILogStore>());
        Null(serviceProvider.GetService<ILogStoreLive>());
        IsType<InsightsLogStore>(serviceProvider.GetRequiredService<IInsightsLogStore>());
        IsType<MelInsightsLogSnapshotReader>(serviceProvider.GetRequiredService<IInsightsLogSnapshotReader>());

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ToSic.App");
        var reader = serviceProvider.GetRequiredService<IInsightsLogSnapshotReader>();
        logger.Log(LogLevel.Warning, default, State(("Segment", "unit")), null, static (state, _) => state[0].Value!.ToString()!);
        reader.Pause();
        logger.Log(LogLevel.Warning, default, State(("Segment", "unit")), null, static (state, _) => state[0].Value!.ToString()!);
        reader.Resume();
        reader.FlushSegment("unit");
        logger.Log(LogLevel.Warning, default, State(("Segment", "unit")), null, static (state, _) => state[0].Value!.ToString()!);

        Equal(3, other.Entries.Count);
        Equal(["unit"], reader.Snapshot().Groups.Single().Events.Select(entry => entry.Message));
    }

    [Fact]
    public void LegacyRegistration_UsesCompleteLegacyStackByDefault()
    {
        var services = new ServiceCollection();
        services.AddSysCore();

        using var serviceProvider = services.BuildServiceProvider();
        IsType<LegacyLogFactory>(serviceProvider.GetRequiredService<ILogFactory>());
        IsType<LogStoreLive>(serviceProvider.GetRequiredService<ILogStore>());
        IsType<LogStoreLive>(serviceProvider.GetRequiredService<ILogStoreLive>());
        IsType<LegacyInsightsLogSnapshotReader>(serviceProvider.GetRequiredService<IInsightsLogSnapshotReader>());
        Null(serviceProvider.GetService<IInsightsLogStore>());
        Empty(serviceProvider.GetServices<ILoggerProvider>().Where(provider => provider is InsightsLoggerProvider));
    }

    private static IReadOnlyList<KeyValuePair<string, object?>> State(params (string Key, object? Value)[] values)
        => values.Select(value => new KeyValuePair<string, object?>(value.Key, value.Value)).ToList();

    private sealed class ThrowingStore : IInsightsLogStore
    {
        public InsightsAppendResult Append(InsightsEvent entry) => throw new InvalidOperationException();
        public void Pause() { }
        public void Resume() { }
        public void FlushGroup(string traceId) { }
        public void FlushSegment(string? segment) { }
        public void Flush() { }
        public InsightsLogStoreSnapshot Snapshot() => throw new NotImplementedException();
        public ImmutableArray<InsightsEvent> ReadGroup(string traceId) => [];
        public ImmutableArray<InsightsEvent> List(string? segment = null) => [];
        public ImmutableArray<InsightsGroupSummary> ListGroups() => [];
    }

    private sealed class RecordingProvider : ILoggerProvider
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];
        public ILogger CreateLogger(string categoryName) => new RecordingLogger(Entries);
        public void Dispose() { }
    }

    private sealed class RecordingLogger(List<(LogLevel Level, string Message)> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => EmptyScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => entries.Add((logLevel, formatter(state, exception)));
    }

    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();
        public void Dispose() { }
    }
}
