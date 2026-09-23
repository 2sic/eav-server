using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

public class MelLogStoreTests
{
    [Fact]
    public void Link_ReusedLogKeepsSequentialRequestSegmentsAndReleasesContexts()
    {
        var (recording, factory) = NewFactory();
        var store = new MelLogStore();
        var reused = new HasLog(factory.Create("App.Reused", null, new CodeRef()));
        var discarded = Enumerable.Range(0, 8)
            .Select(index => LinkAndWriteInIsolatedExecution(factory, store, reused, index))
            .ToList();

        ForceCollection();

        Equal(Enumerable.Range(0, 8).Select(index => $"request-{index}"),
            recording.Entries
                .Where(entry => entry.Value("Message")?.ToString()?.StartsWith("reused-") == true)
                .Select(entry => entry.Value("Segment")));
        DoesNotContain(discarded, context => context.TryGetTarget(out _));
    }

    [Fact]
    public void Link_ReusedLogKeepsParallelRequestSegmentsIsolated()
    {
        var (recording, factory) = NewFactory();
        var store = new MelLogStore();
        var reused = new HasLog(factory.Create("App.Reused", null, new CodeRef()));

        Parallel.For(0, 16, index => LinkAndWriteInIsolatedExecution(factory, store, reused, index));
        reused.Log.A("after-parallel");

        var segments = recording.Entries
            .Where(entry => entry.Value("Message")?.ToString()?.StartsWith("reused-") == true)
            .ToDictionary(entry => entry.Value("Message")!.ToString()!, entry => entry.Value("Segment"));
        Equal(16, segments.Count);
        foreach (var index in Enumerable.Range(0, 16))
            Equal($"request-{index}", segments[$"reused-{index}"]);
        Null(Single(recording.Entries, entry => Equals(entry.Value("Message"), "after-parallel")).Value("Segment"));
    }

    [Fact]
    public void Link_LeavesEarlierAdmittedRootSegmentStable()
    {
        var (recording, factory) = NewFactory();
        var store = new MelLogStore();
        var reused = new HasLog(factory.Create("App.Reused", null, new CodeRef()));
        var first = factory.Create("App.First", null, new CodeRef());
        store.Add("request-first", first);
        reused.LinkLog(first);
        first.A("first-before");

        var second = factory.Create("App.Second", null, new CodeRef());
        store.Add("request-second", second);
        reused.LinkLog(second);
        first.A("first-after");

        Equal(["request-first", "request-first"], recording.Entries
            .Where(entry => entry.Value("Message")?.ToString()?.StartsWith("first-") == true)
            .Select(entry => entry.Value("Segment")));
    }

    [Fact]
    public void Add_ExplicitlyAdmittedLogKeepsSegmentOutsideAdmittingExecution()
    {
        var (recording, factory) = NewFactory();
        var log = factory.Create("App.Background", null, new CodeRef());
        new MelLogStore().Add("startup", log);

        using (ExecutionContext.SuppressFlow())
            Task.Run(() => log.A("background")).GetAwaiter().GetResult();

        Equal("startup", Single(recording.Entries).Value("Segment"));
    }

    [Fact]
    public void AddSpec_LateEmitsOneMetadataEventWithoutReplay()
    {
        var (recording, log) = NewLog();
        log.A("before");
        var entry = new MelLogStore().Add("webapi", log)!;
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();

        entry.AddSpec("User", "tonci");

        Equal(2, recording.Entries.Count);
        var metadata = recording.Entries[1];
        Equal(LogLevel.Trace, metadata.Level);
        Equal("Log specs", metadata.Value("Message"));
        Equal("webapi", metadata.Value("Segment"));
        Equal("tonci", metadata.Value("User"));
        Equal(activity.TraceId.ToString(), metadata.Value("TraceId"));
        Equal(activity.SpanId.ToString(), metadata.Value("SpanId"));
    }

    [Fact]
    public void UpdateSpecs_EmitsOnceWithCompleteUnchangedValues()
    {
        var (recording, log) = NewLog();
        var entry = new MelLogStore().ForceAdd("module", log)!;
        entry.AddSpec("User", "tonci");
        recording.Entries.Clear();
        const string url = "https://example.test/path?a=one&b=two%20words";

        entry.UpdateSpecs(new Dictionary<string, string>
        {
            ["Url"] = url,
            ["Path"] = "/path?a=one&b=two%20words"
        });

        var metadata = Single(recording.Entries);
        Equal("module", metadata.Value("Segment"));
        Equal("tonci", metadata.Value("User"));
        Equal(url, metadata.Value("Url"));
        Equal("/path?a=one&b=two%20words", metadata.Value("Path"));

        entry.UpdateSpecs(new Dictionary<string, string> { ["Url"] = url });
        Equal(1, recording.Entries.Count);
    }

    [Fact]
    public void Add_RejectsLegacyLog()
        => Null(new MelLogStore().Add("segment", new Log("legacy")));

    [Fact]
    public void Add_AssignsSegmentToSubsequentEvents()
    {
        var (recording, log) = NewLog();
        new MelLogStore().Add("webapi", log);

        log.A("after admission");

        Equal("webapi", Single(recording.Entries).Value("Segment"));
    }

    [Fact]
    public void MelInsights_SegmentEventsAreVisibleAndFlushable_WithoutHostLogging()
    {
        var services = new ServiceCollection();
        services.AddSysCoreMelInsightsLogging();
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILogFactory>();
        var log = factory.Create("App.Log", null, new CodeRef());
        var factoryChild = factory.Create("App.FactoryChild", log, new CodeRef());
        var linkedParent = new HasLog(factory.Create("App.LinkedParent", null, new CodeRef()));
        var linkedChild = new HasLog(factory.Create("App.LinkedChild", null, new CodeRef())).LinkLog(linkedParent.Log).Log;
        linkedParent.LinkLog(log);
        // Existing DI may connect the same family again; this reverse link must stay cycle-safe.
        new HasLog(log).LinkLog(linkedChild);
        provider.GetRequiredService<ILogStore>().Add("webapi", log);
        factoryChild.A("factory child");
        linkedChild.A("linked child");
        var reader = provider.GetRequiredService<IInsightsLogSnapshotReader>();

        Equal(2, reader.Snapshot().Groups.SelectMany(group => group.Events).Count(entry => entry.Message?.EndsWith(" child") == true));
        reader.FlushSegment("webapi");
        Empty(reader.Snapshot().Groups.SelectMany(group => group.Events));
    }

    [Fact]
    public void CoreOnlyMel_RegistersWithoutInsightsAndKeepsOtherProvider()
    {
        var other = new OtherProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddProvider(other));
        services.AddSysCoreMelLogging();
        using var provider = services.BuildServiceProvider();

        IsType<MelLogFactory>(provider.GetRequiredService<ILogFactory>());
        IsType<MelLogStore>(provider.GetRequiredService<ILogStore>());
        Null(provider.GetService<IInsightsLogStore>());
        Null(provider.GetService<IInsightsLogSnapshotReader>());
        Null(provider.GetService<InsightsLoggerProvider>());
        Same(other, Single(provider.GetServices<ILoggerProvider>()));
    }

    [Fact]
    public void CoreOnlyMel_PreservesExistingHostFactory()
    {
        using var hostFactory = new MelLogTests.RecordingLoggerFactory(true);
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(hostFactory);
        services.AddSysCoreMelLogging();
        using var provider = services.BuildServiceProvider();

        var log = provider.GetRequiredService<ILogFactory>().Create("App.Log", null, new CodeRef());
        log.A("host event");

        Same(hostFactory, Single(provider.GetServices<ILoggerFactory>()));
        Equal("host event", Single(hostFactory.Entries).Value("Message"));
    }

    [Fact]
    public void Registration_UsesLegacyByDefault()
    {
        var legacy = new ServiceCollection().AddSysCoreLogging();
        Equal(ServiceLifetime.Singleton, Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogFactory)).Lifetime);
        Equal(typeof(LegacyLogFactory), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogFactory)).ImplementationInstance?.GetType());
        Equal(typeof(LogStoreLive), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogStore)).ImplementationType);
        Equal(typeof(LogStoreLive), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogStoreLive)).ImplementationType);
        Equal(typeof(LegacyInsightsLogSnapshotReader), Single(legacy, descriptor => descriptor.ServiceType == typeof(IInsightsLogSnapshotReader)).ImplementationType);
    }

    private static (MelLogTests.RecordingLoggerFactory Recording, ILog Log) NewLog()
    {
        var recording = new MelLogTests.RecordingLoggerFactory(true);
        return (recording, new MelLogFactory(recording).Create("App.Log", null, new CodeRef()));
    }

    private static (MelLogTests.RecordingLoggerFactory Recording, MelLogFactory Factory) NewFactory()
    {
        var recording = new MelLogTests.RecordingLoggerFactory(true);
        return (recording, new(recording));
    }

    private static WeakReference<MelSegmentContext> LinkAndWriteInIsolatedExecution(MelLogFactory factory, MelLogStore store, HasLog reused, int index)
    {
        WeakReference<MelSegmentContext>? weak = null;
        ExecutionContext.Run(ExecutionContext.Capture()!, _ =>
        {
            var root = factory.Create($"App.Request{index}", null, new CodeRef());
            store.Add($"request-{index}", root);
            reused.LinkLog(root);
            reused.Log.A($"reused-{index}");
            weak = new(IsType<MelLog>(root).SegmentContext);
        }, null);
        return weak!;
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class OtherProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public void Dispose() { }
    }

    private sealed class HasLog(ILog log) : IHasLog
    {
        public ILog Log { get; } = log;
    }
}
