using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.Persistence.File;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsGlobalTypesLog(LazySvc<ILogStoreLive> logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [logStore])
{
    public static string Link = "GlobalTypesLog";

    public override string Title => "Global Types Log";

    private InsightsLogsHelper LogHtml => _logHtml ??= new(logStore.Value);
    private InsightsLogsHelper _logHtml;

    public override string HtmlBody()
    {
        var msg = InsightsHtmlParts.PageStyles() + LogHtml.LogHeader(null, false);
        var log = InternalAppLoader.LoadLog;
        return msg + (log == null
            ? P("log is null").ToString()
            : LogHtml.DumpTree("Log for Global Types loading", log));
    }

}