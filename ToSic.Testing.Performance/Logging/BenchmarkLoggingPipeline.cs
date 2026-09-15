using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToSic.Sys.Logging;

namespace ToSic.Testing.Performance.Logging;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 8, invocationCount: 1)]
public class BenchmarkLoggingPipeline
{
    private const int EntryCount = 1_000;
    private const int OwnershipCount = 100;

    [Params(
        LoggingPipelineMode.DiagnosticNoSink,
        LoggingPipelineMode.DiagnosticNoStoreProvider,
        LoggingPipelineMode.InsightsOneBundle,
        LoggingPipelineMode.InsightsParentAndChild)]
    public LoggingPipelineMode Mode { get; set; }

    private Log _log = null!;
    private Log _reused = null!;
    private ILoggerFactory _factory;
    private ILogger _logger = null!;
    private LogStoreLive _store;
    private LogStoreEntry _execution;
    private int _expectedEntries;
    private static readonly System.Diagnostics.ActivitySource Activities = new(LogExecution.ActivitySourceName);

    [IterationSetup]
    public void Setup()
    {
        LogEventBridge.SetSink(null);
        var root = new Log("Perf.Root");
        _log = root;
        _reused = new("Perf.Reused");
        _expectedEntries = -1;

        if (Mode == LoggingPipelineMode.DiagnosticNoSink)
        {
            CreateFactory();
            return;
        }

        if (Mode == LoggingPipelineMode.DiagnosticNoStoreProvider)
        {
            EnableBridge();
            return;
        }

        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        EnableBridge(provider);
        _store = new(memory, provider);
        _store.Configure(null);
        _execution = _store.Add("http-request", root);
        if (Mode == LoggingPipelineMode.InsightsOneBundle)
            return;

        _log = new Log("Perf.Child");
        _execution = _store.Add("module", _log);
    }

    private void EnableBridge(ILoggerProvider provider = null)
    {
        _factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            if (provider != null)
                builder.AddProvider(provider);
        });
        _logger = _factory.CreateLogger(MicrosoftLoggerEventSink.Category);
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(_factory));
    }

    private void CreateFactory()
    {
        _factory = LoggerFactory.Create(_ => { });
        _logger = _factory.CreateLogger(MicrosoftLoggerEventSink.Category);
    }

    [Benchmark(OperationsPerInvoke = EntryCount)]
    public void WriteEntries()
    {
        for (var i = 0; i < EntryCount; i++)
            _log.A("benchmark entry");
        _expectedEntries = EntryCount;
    }

    [Benchmark(OperationsPerInvoke = OwnershipCount)]
    public void FnDone()
    {
        for (var i = 0; i < OwnershipCount; i++)
            _log.Fn(timer: true).Done("ok");
        _expectedEntries = OwnershipCount;
    }

    [Benchmark(OperationsPerInvoke = OwnershipCount)]
    public void FnDispose()
    {
        for (var i = 0; i < OwnershipCount; i++)
            using (_log.Fn(timer: true)) { }
        _expectedEntries = OwnershipCount;
    }

    [Benchmark(OperationsPerInvoke = OwnershipCount)]
    public void NestedOperationsAcrossServices()
    {
        for (var i = 0; i < OwnershipCount; i++)
        {
            using var parent = _log.Fn("parent");
            using var child = _reused.Fn("child");
            child.Done("child");
            parent.Done("parent");
        }
        _expectedEntries = OwnershipCount * 2;
    }

    [Benchmark(OperationsPerInvoke = OwnershipCount)]
    public async Task AsyncReusedServiceContext()
    {
        using var execution = _execution != null
            ? _logger.BeginExecution(_execution, Activities, "benchmark")
            : _logger.BeginExecution(_log, Activities, "benchmark");
        for (var i = 0; i < OwnershipCount; i++)
        {
            using var call = _reused.Fn("async");
            await Task.Yield();
            call.Done("ok");
        }
        _expectedEntries = OwnershipCount;
    }

    [Benchmark(OperationsPerInvoke = OwnershipCount)]
    public void ExecutionScopeSetup()
    {
        for (var i = 0; i < OwnershipCount; i++)
            using (_logger.BeginExecution(_log, Activities, "benchmark")) { }
        _expectedEntries = 0;
    }

    [IterationCleanup]
    public void Cleanup()
    {
        if (_store != null && _expectedEntries >= 0)
        {
            var actual = new[] { "http-request", "module" }
                .SelectMany(_store.Snapshot)
                .SelectMany(snapshot => snapshot.Entries)
                .Select(entry => entry.Sequence)
                .Distinct()
                .Count();
            if (actual != _expectedEntries)
                throw new InvalidOperationException($"Captured {actual} entries; expected {_expectedEntries}.");
        }
        LogEventBridge.SetSink(null);
        _factory?.Dispose();
        _factory = null;
        _store = null;
        _execution = null;
    }
}

public enum LoggingPipelineMode
{
    DiagnosticNoSink,
    DiagnosticNoStoreProvider,
    InsightsOneBundle,
    InsightsParentAndChild,
}
