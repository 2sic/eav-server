using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;
using ToSic.Sys.DI;
using ToSic.Sys.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Eav.Insights;

internal sealed class InsightsLogsTestContext : IDisposable
{
    internal ILogStoreLive Store { get; }
    private readonly ServiceProvider _services;
    internal InsightsLogs View { get; }

    internal InsightsLogsTestContext(LogStoreMode mode = LogStoreMode.Legacy)
    {
        _services = new ServiceCollection().AddSysCoreLogging().BuildServiceProvider();
        Store = _services.GetRequiredService<ILogStoreLive>();
        Store.SegmentSize = 2;
        LogEventBridge.SetSink(new MicrosoftLoggerEventSink(_services.GetRequiredService<ILoggerFactory>()));
        Store.Configure(mode.ToString(), bridgeEnabled: true);
        View = new(new LazySvc<ILogStoreLive>(_services));
    }

    internal Log Add(string message, string moduleId)
    {
        var log = new Log("Tst.View");
        log.A(message);
        Store.Add("test", log)!.AddSpec("ModuleId", moduleId);
        return log;
    }

    internal void SetContext(string? logId = null, string? filter = null, int? position = null)
        => View.SetContext(new InsightsHtmlTable(), null, new Dictionary<string, object?>(),
            "test", position, null!, null, logId!, filter!);

    public void Dispose()
    {
        LogEventBridge.SetSink(null);
        _services.Dispose();
    }
}
