using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;
using ToSic.Sys.DI;
using ToSic.Sys.Logging;

namespace ToSic.Eav.Insights;

internal sealed class InsightsLogsTestContext : IDisposable
{
    private readonly LogStoreLive _store = new() { SegmentSize = 2 };
    private readonly ServiceProvider _services;
    internal InsightsLogs View { get; }

    internal InsightsLogsTestContext()
    {
        _services = new ServiceCollection().AddSingleton<ILogStoreLive>(_store).BuildServiceProvider();
        View = new(new LazySvc<ILogStoreLive>(_services));
    }

    internal Log Add(string message, string moduleId)
    {
        var log = new Log("Tst.View");
        log.A(message);
        _store.Add("test", log)!.AddSpec("ModuleId", moduleId);
        return log;
    }

    internal void SetContext(string? logId = null, string? filter = null, int? position = null)
        => View.SetContext(new InsightsHtmlTable(), null, new Dictionary<string, object?>(),
            "test", position, null!, null, logId!, filter!);

    public void Dispose() => _services.Dispose();
}
