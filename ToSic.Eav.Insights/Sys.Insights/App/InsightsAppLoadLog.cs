using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsAppLoadLog(LazySvc<IAppStateCacheService> appStates, LazySvc<ILogStoreLive> logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates, logStore])
{
    private InsightsLogsHelper LogHtml => field ??= new(logStore.Value);

    public static string Link = "AppLoadLog";

    public override string Title => "App Load Log";

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