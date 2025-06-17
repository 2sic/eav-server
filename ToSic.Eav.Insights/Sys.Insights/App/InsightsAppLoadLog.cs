using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsAppLoadLog(LazySvc<IAppStateCacheService> appStates, LazySvc<ILogStoreLive> logStore)
    : InsightsProvider(new() { Name = Link, Title = "App Load Log" }, connect: [appStates, logStore])
{
    [field: AllowNull, MaybeNull]
    private InsightsLogsHelper LogHtml => field ??= new(logStore.Value);

    public static string Link = "AppLoadLog";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, out var message))
            return message;

        Log.A($"debug app-load {AppId}");
        return InsightsHtmlParts.PageStyles()
               + LogHtml.DumpTree(
                   $"2sxc load log for app {AppId}",
                   appStates.Value.Get(AppId.Value).Log
               );
    }

}