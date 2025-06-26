using ToSic.Eav.Apps.Sys.AppStateInFolder;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsGlobalTypesLog(LazySvc<ILogStoreLive> logStore)
    : InsightsProvider(new() { Name = Link, Title = "Global Types Log" }, connect: [logStore])
{
    public static string Link = "GlobalTypesLog";

    [field: AllowNull, MaybeNull]
    private InsightsLogsHelper LogHtml => field ??= new(logStore.Value);

    public override string HtmlBody()
    {
        var msg = InsightsHtmlParts.PageStyles() + LogHtml.LogHeader(Link, false);
        var log = AppStateInFolderGlobalLog.LoadLog;
        return msg + (log == null
            ? P("log is null").ToString()
            : LogHtml.DumpTree("Log for Global Types loading", log));
    }

}