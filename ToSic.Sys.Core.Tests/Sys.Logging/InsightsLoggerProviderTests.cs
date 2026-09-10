using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

[Collection(nameof(LogEventBridgeTests))]
public class InsightsLoggerProviderTests
{
    [Fact]
    public void StoreLogger_DisablesWrites_InLegacyMode()
    {
        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        var store = new LogStoreLive(memory, provider);
        var logger = provider.CreateLogger("Test.Native");

        store.Configure("ILogger", bridgeEnabled: true);
        True(logger.IsEnabled(LogLevel.Information));

        store.Configure("Legacy", bridgeEnabled: true);
        False(logger.IsEnabled(LogLevel.Information));
        False(logger.IsEnabled(LogLevel.None));
    }

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
        var memory = services.GetRequiredService<InsightsLogStore>();
        Same(store, services.GetRequiredService<ILogStore>());
        var factory = services.GetRequiredService<ILoggerFactory>();
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        try
        {
            store.Configure("ILogger", bridgeEnabled: true);
            var log = new Log("Tst.Late");
            False(memory.Knows(log.LogId, []));
            log.A("before admission");
            store.Add("search", log);
            True(memory.Knows(log.LogId, []));
            var logger = factory.CreateLogger("Test.Native");
            using (logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["2sxc.LogId"] = log.LogId,
                       ["AppId"] = 42,
                   }))
                logger.Log(LogLevel.Information, new EventId(7, "Native.Kind"), "native {Value}", 7);

            var snapshot = Single(store.Snapshot("search"));
            var replayed = Single(snapshot.Entries, e => e.Message == "before admission");
            Equal(DateTimeKind.Utc, replayed.Created.Kind);
            var native = Single(snapshot.Entries, e => e.Message == "native 7");
            Equal(DateTimeKind.Utc, native.Created.Kind);
            Equal("42", native.Properties["AppId"]);
            Equal("7", native.Properties["EventId"]);
            Equal("Native.Kind", native.Properties["EventName"]);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_ReplaysExceptionDetails_WhenAdmittedAfterThrow()
    {
        using var services = new ServiceCollection().AddSysCoreLogging().BuildServiceProvider();
        var store = services.GetRequiredService<ILogStoreLive>();
        var factory = services.GetRequiredService<ILoggerFactory>();
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        try
        {
            store.Configure("ILogger", bridgeEnabled: true);
            var log = new Log("Tst.Error");
            var exception = Throws<InvalidOperationException>((Action)(() =>
                throw new InvalidOperationException("before admission")));
            log.Ex(exception);

            store.Add("exceptions", log);

            var entry = Single(Single(store.Snapshot("exceptions")).Entries, e => e.ExceptionType != null);
            Equal(exception.GetType().FullName, entry.ExceptionType);
            Equal(exception.ToString(), entry.ExceptionText);
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
            Contains(memory.Find(child.LogId)!.Entries, entry => entry.Message == "written before link");
            Null(memory.Find("unknown"));

            var second = new Log("Tst.Two");
            var third = new Log("Tst.Three");
            store.Add("module", second);
            store.Add("module", third);

            var remaining = store.Snapshot("module");
            Equal(2, remaining.Count);
            DoesNotContain(remaining, snapshot => snapshot.LogId == root.LogId);
            Null(memory.Find(child.LogId));
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public async Task Store_AmbientScope_AttachesUnlinkedLogsAcrossAwaitWithoutContextLeak()
    {
        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        var store = new LogStoreLive(memory, provider);
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        using var source = new ActivitySource("Test.2sxc.Scope");
        using var listener = new ActivityListener
        {
            ShouldListenTo = candidate => candidate.Name == source.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        try
        {
            store.Configure("ILogger", bridgeEnabled: true);
            var rootA = new Log("Tst.RootA");
            var rootB = new Log("Tst.RootB");
            store.Add("scope", rootA);
            store.Add("scope", rootB);
            var logger = factory.CreateLogger("Test.Scope");

            await Task.WhenAll(WriteInScope(rootA, "child-a"), WriteInScope(rootB, "child-b"));
            await ThrowsAsync<InvalidOperationException>(() => ThrowInScope(rootA));
            logger.LogInformation("outside scope");

            var snapshotA = Single(store.Snapshot("scope"), snapshot => snapshot.LogId == rootA.LogId);
            var snapshotB = Single(store.Snapshot("scope"), snapshot => snapshot.LogId == rootB.LogId);
            Contains(snapshotA.Entries, entry => entry.Message == "child-a" && entry.Ancestors.Contains(rootA.LogId));
            DoesNotContain(snapshotA.Entries, entry => entry.Message is "child-b" or "outside scope");
            Contains(snapshotB.Entries, entry => entry.Message == "child-b" && entry.Properties.ContainsKey("TraceId"));

            async Task WriteInScope(Log root, string message)
            {
                using var scope = logger.BeginExecution(root, source, "pilot", moduleId: 333);
                await Task.Yield();
                var child = new Log("Tst.Unlinked");
                Null(child.Parent);
                child.A(message);
                logger.LogInformation("native {Message}", message);
            }

            async Task ThrowInScope(Log root)
            {
                using var scope = logger.BeginExecution(root, source, "throws");
                await Task.Yield();
                throw new InvalidOperationException("expected");
            }
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public async Task Store_AmbientScope_CancellationDoesNotLeakToLaterBackgroundWork()
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
            store.Configure("ILogger", bridgeEnabled: true);
            var root = new Log("Tst.Cancelled");
            store.Add("scope", root);
            var logger = factory.CreateLogger("Test.Scope");

            await ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                using var scope = logger.BeginScope(new Dictionary<string, object?>
                {
                    ["2sxc.LogId"] = root.LogId,
                    ["2sxc.AmbientLogId"] = root.LogId,
                });
                logger.LogInformation("before cancellation");
                await Task.Yield();
                throw new OperationCanceledException();
            });
            await Task.Run(() => logger.LogInformation("background after cancellation"));

            var snapshot = Single(store.Snapshot("scope"));
            Contains(snapshot.Entries, entry => entry.Message == "before cancellation");
            DoesNotContain(snapshot.Entries, entry => entry.Message == "background after cancellation");
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public async Task Store_AmbientScope_SuppressedExecutionContextDetachesBackgroundWork()
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
            store.Configure("ILogger", bridgeEnabled: true);
            var root = new Log("Tst.Detached");
            store.Add("scope", root);
            var logger = factory.CreateLogger("Test.Scope");
            Task background;

            using (logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["2sxc.LogId"] = root.LogId,
                       ["2sxc.AmbientLogId"] = root.LogId,
                   }))
            {
                logger.LogInformation("inside request");
                using (ExecutionContext.SuppressFlow())
                    background = Task.Run(() => logger.LogInformation("detached background"));
            }
            await background;

            var snapshot = Single(store.Snapshot("scope"));
            Contains(snapshot.Entries, entry => entry.Message == "inside request");
            DoesNotContain(snapshot.Entries, entry => entry.Message == "detached background");
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_KeepsFirstCapturedContext_AndDisclosesTruncation_WhenPropertyBudgetIsExhausted()
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
            store.Configure("ILogger", bridgeEnabled: true);
            var log = new Log("Tst.Budget");
            var handle = store.Add("budget", log)!;
            var logger = factory.CreateLogger("Test.Native");
            var noise = Enumerable.Range(0, InsightsLogStore.MaxProperties * 2)
                .ToDictionary(i => "Noise" + i, i => (object?)i);

            using (logger.BeginScope(noise))
            using (logger.BeginScope(new Dictionary<string, object?>
                   {
                       [LogExecution.LogIdKey] = log.LogId,
                       [InsightsLoggerProvider.CodeMemberKey] = "Load",
                       [InsightsLoggerProvider.CodeFileKey] = @"C:\Src\EditController.cs",
                       [InsightsLoggerProvider.CodeLineKey] = 42,
                   }))
                logger.LogInformation("native beyond the property budget");

            handle.AddSpec("SiteId", "1");
            for (var i = 0; i < InsightsLogStore.MaxProperties * 2; i++)
                handle.AddSpec("Spec" + i, i.ToString());

            var snapshot = Single(store.Snapshot("budget"));
            var native = Single(snapshot.Entries);
            Equal("true", native.Properties[InsightsLogStore.TruncatedKey]);
            Equal("Load", native.Code!.Name);
            Equal(42, native.Code.Line);
            Equal(@"C:\Src\EditController.cs", native.Code.Path);
            // Late specs must not silently push out the identity captured at admission.
            Equal("1", snapshot.Specs["SiteId"]);
            Equal("true", snapshot.Specs[InsightsLogStore.TruncatedKey]);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_CapsOneBundleAtConfiguredEntryLimit()
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
            store.Configure("ILogger", bridgeEnabled: true);
            var log = new Log("Tst.Cap");
            store.Add("cap", log);

            for (var i = 0; i <= InsightsLogStore.MaxEntriesPerLog; i++)
                log.A("entry");

            var snapshot = Single(store.Snapshot("cap"));
            Equal(InsightsLogStore.MaxEntriesPerLog, snapshot.Entries.Length);
            Equal(1, snapshot.DroppedEntries);
        }
        finally
        {
            LogEventBridge.SetSink(null);
        }
    }

    [Fact]
    public void Store_ChargesDeepTreeEventOnce_AgainstByteBudget()
    {
        var memory = new InsightsLogStore { Enabled = true };
        var logIds = Enumerable.Range(0, 5).Select(i => $"log-{i}").ToArray();
        foreach (var logId in logIds)
            memory.Write(new() { Kind = "Admission", LogId = logId, Segment = "depth" }, logIds.Length);
        var message = new string('x', InsightsLogStore.MaxTextLength);

        for (var sequence = 1; sequence <= 400; sequence++)
            memory.Write(new()
            {
                LogId = logIds[4], Ancestors = [logIds[3], logIds[2], logIds[1], logIds[0]],
                Sequence = sequence, Source = "Tst.Depth", ShortSource = "Tst.Depth", Message = message,
            }, logIds.Length);

        Equal(logIds.Length, memory.Snapshot("depth").Count);
    }

    [Fact(Timeout = 2000)]
    public async Task Store_EvictsSpecLessBundle_WithoutHanging()
    {
        var memory = new InsightsLogStore { Enabled = true };
        const string logId = "spec-less";
        memory.Write(new() { Kind = "Admission", LogId = logId, Segment = "test" }, 1);
        const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        var logs = (IDictionary)typeof(InsightsLogStore).GetField("_logs", privateInstance)!.GetValue(memory)!;
        var bundle = logs[logId]!;
        var specs = (IDictionary)bundle.GetType().GetProperty("Specs")!.GetValue(bundle)!;
        specs.Clear();
        typeof(InsightsLogStore).GetField("_bytes", privateInstance)!.SetValue(memory, InsightsLogStore.MaxEstimatedBytes + 1);

        await Task.Run(() => typeof(InsightsLogStore).GetMethod("EnforceBudget", privateInstance)!.Invoke(memory, null));

        Null(memory.Find(logId));
    }

    [Fact]
    public void Configure_RemovedCompareMode_FallsBackToLegacy()
    {
        var store = new LogStoreLive();
        Equal("Unknown logging store; retaining Legacy.", store.Configure("Compare", bridgeEnabled: true));
        Equal(LogStoreMode.Legacy, store.Mode);
    }
}
