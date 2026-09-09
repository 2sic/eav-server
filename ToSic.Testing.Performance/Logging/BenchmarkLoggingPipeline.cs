using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Logging;

namespace ToSic.Testing.Performance.Logging;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 8, invocationCount: 1)]
public class BenchmarkLoggingPipeline
{
    private const int EntryCount = 1_000;

    [Params(
        LoggingPipelineMode.BridgeOff,
        LoggingPipelineMode.ILoggerNoProvider,
        LoggingPipelineMode.InsightsOneBundle,
        LoggingPipelineMode.InsightsParentAndChild)]
    public LoggingPipelineMode Mode { get; set; }

    private Log _log = null!;
    private ILoggerFactory _factory;

    [IterationSetup]
    public void Setup()
    {
        LogEventBridge.SetSink(null);
        var root = new Log("Perf.Root");
        _log = root;

        if (Mode == LoggingPipelineMode.BridgeOff)
            return;

        if (Mode == LoggingPipelineMode.ILoggerNoProvider)
        {
            EnableBridge();
            return;
        }

        var memory = new InsightsLogStore();
        var provider = new InsightsLoggerProvider(memory);
        EnableBridge(provider);
        var store = new LogStoreLive(memory, provider);
        store.Configure("ILogger", bridgeEnabled: true);
        store.Add("http-request", root);
        if (Mode == LoggingPipelineMode.InsightsOneBundle)
            return;

        _log = new Log("Perf.Child", root);
        store.Add("module", _log);
    }

    private void EnableBridge(ILoggerProvider provider = null)
    {
        _factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            if (provider != null)
                builder.AddProvider(provider);
        });
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(_factory));
    }

    [Benchmark(OperationsPerInvoke = EntryCount)]
    public void WriteEntries()
    {
        for (var i = 0; i < EntryCount; i++)
            _log.A("benchmark entry");
    }

    [IterationCleanup]
    public void Cleanup()
    {
        LogEventBridge.SetSink(null);
        _factory?.Dispose();
        _factory = null;
    }
}

public enum LoggingPipelineMode
{
    BridgeOff,
    ILoggerNoProvider,
    InsightsOneBundle,
    InsightsParentAndChild,
}
