using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

[Collection(nameof(LogEventBridgeTests))]
public class InsightsLoggerProviderTests
{
    [Fact]
    public void Store_PreservesTreeTimingCodeAndLateSpecs_ThroughILogger()
    {
        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        var store = new LogStoreLive(memory, provider);
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        try
        {
            Equal("ILogger", store.Configure("ILogger", bridgeEnabled: true).Split(' ')[0]);
            var log = new Log("Tst.Store");
            var handle = store.Add("module", log)!;
            var outer = log.Fn(message: "outer", timer: true);
            outer.A("inside");
            var inner = outer.Fn(message: "inner", timer: true);
            inner.W("warning");
            inner.Done("inner done");
            outer.Done("outer done");
            handle.AddSpec("AppId", "42");

            var snapshot = Single(store.Snapshot("module"));
            Equal(4, snapshot.Entries.Length);
            Equal("42", snapshot.Specs["AppId"]);
            Equal("outer done", snapshot.Entries.Single(e => e.WrapOpen && e.Message!.Contains("outer")).Result);
            var innerStart = snapshot.Entries.Single(e => e.WrapOpen && e.Message!.Contains("inner"));
            NotNull(innerStart.ParentOperationId);
            Equal(LogLevel.Warning, snapshot.Entries.Single(e => e.Message!.Contains("warning")).Level);
            NotNull(snapshot.Entries.Single(e => e.Message!.Contains("inside")).Code);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_ReplaysEntriesCreatedBeforeAdmission_AndCapturesNativeScope()
    {
        using var services = new ServiceCollection().AddSysCoreLogging().BuildServiceProvider();
        var store = services.GetRequiredService<ILogStoreLive>();
        Same(store, services.GetRequiredService<ILogStore>());
        var factory = services.GetRequiredService<ILoggerFactory>();
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        try
        {
            store.Configure("ILogger", bridgeEnabled: true);
            var log = new Log("Tst.Late");
            log.A("before admission");
            store.Add("search", log);
            var logger = factory.CreateLogger("Test.Native");
            using (logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["2sxc.LogId"] = log.LogId,
                       ["AppId"] = 42,
                   }))
                logger.LogInformation("native {Value}", 7);

            var snapshot = Single(store.Snapshot("search"));
            Contains(snapshot.Entries, e => e.Message == "before admission");
            var native = Single(snapshot.Entries, e => e.Message == "native 7");
            Equal("42", native.Properties["AppId"]);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_ReplaysLateChildLink_AndEvictsWholeOldLogFromSegment()
    {
        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        var store = new LogStoreLive(memory, provider) { SegmentSize = 2 };
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        try
        {
            store.Configure("ILogger", bridgeEnabled: true);
            var root = new Log("Tst.Root");
            store.Add("module", root);
            var call = root.Fn(message: "parent");
            var child = new Log("Tst.Child");
            child.A("written before link");
            child.LinkTo(call);
            call.Done();

            Contains(Single(store.Snapshot("module")).Entries,
                entry => entry.Message == "written before link" && entry.OperationId == call.Entry!.Sequence);

            var second = new Log("Tst.Two");
            var third = new Log("Tst.Three");
            store.Add("module", second);
            store.Add("module", third);

            var remaining = store.Snapshot("module");
            Equal(2, remaining.Count);
            DoesNotContain(remaining, snapshot => snapshot.LogId == root.LogId);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }
}
