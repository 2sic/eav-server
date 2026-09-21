using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

public class MelLogStoreTests
{
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
    public void Registrations_KeepLegacyDefaultAndSwitchExplicitStacks()
    {
        var legacy = new ServiceCollection().AddSysCoreLogging();
        Equal(ServiceLifetime.Singleton, Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogFactory)).Lifetime);
        Equal(typeof(LegacyLogFactory), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogFactory)).ImplementationInstance?.GetType());
        Equal(typeof(LogStoreLive), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogStore)).ImplementationType);
        Equal(typeof(LogStoreLive), Single(legacy, descriptor => descriptor.ServiceType == typeof(ILogStoreLive)).ImplementationType);

        var recording = new MelLogTests.RecordingLoggerFactory(true);
        var mel = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(recording)
            .AddSysCoreMelLogging();
        using var provider = mel.BuildServiceProvider();
        IsType<MelLogFactory>(provider.GetRequiredService<ILogFactory>());
        IsType<MelLogStore>(provider.GetRequiredService<ILogStore>());
        Null(provider.GetService<ILogStoreLive>());

        var legacyAfterMel = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(recording)
            .AddSysCoreMelLogging()
            .AddSysCoreLegacyLogging();
        using var legacyProvider = legacyAfterMel.BuildServiceProvider();
        IsType<LegacyLogFactory>(legacyProvider.GetRequiredService<ILogFactory>());
        IsType<LogStoreLive>(legacyProvider.GetRequiredService<ILogStore>());
        IsType<LogStoreLive>(legacyProvider.GetRequiredService<ILogStoreLive>());

        var melAfterLegacy = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(recording)
            .AddSysCoreLegacyLogging()
            .AddSysCoreMelLogging();
        using var melProvider = melAfterLegacy.BuildServiceProvider();
        IsType<MelLogFactory>(melProvider.GetRequiredService<ILogFactory>());
        IsType<MelLogStore>(melProvider.GetRequiredService<ILogStore>());
        Null(melProvider.GetService<ILogStoreLive>());
    }

    private static (MelLogTests.RecordingLoggerFactory Recording, ILog Log) NewLog()
    {
        var recording = new MelLogTests.RecordingLoggerFactory(true);
        return (recording, new MelLogFactory(recording).Create("App.Log", null, new CodeRef()));
    }
}
