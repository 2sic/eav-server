using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

internal sealed class LogExecutionTestContext : IDisposable
{
    private readonly ServiceProvider _services = new ServiceCollection().AddSysCoreLogging().BuildServiceProvider();
    internal ActivitySource Source { get; } = new("Test.Execution");
    internal ILogStoreLive Store { get; }
    internal ILogger Logger { get; }

    internal LogExecutionTestContext(LogStoreMode mode = LogStoreMode.ILogger)
    {
        var factory = _services.GetRequiredService<ILoggerFactory>();
        Logger = factory.CreateLogger(MicrosoftLoggerEventSink.Category);
        Store = _services.GetRequiredService<ILogStoreLive>();
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(factory));
        Store.Configure(mode.ToString(), bridgeEnabled: true);
    }

    internal Log Admit(string name)
    {
        var log = new Log("Tst." + name);
        Store.Add("test", log);
        return log;
    }

    public void Dispose()
    {
        LogEventBridge.SetSink(null);
        Source.Dispose();
        _services.Dispose();
    }
}
