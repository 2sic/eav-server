using ToSic.Eav.Apps.Internal.Insights;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsAppLoadLog(LazySvc<IAppStates> appStates, LazySvc<ILogStoreLive> logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates, logStore])
{
    private InsightsLogsHelper LogHtml => _logHtml ??= new(logStore.Value);
    private InsightsLogsHelper _logHtml;

    public static string Link = "AppLoadLog";

    public override string Title => "App Load Log";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, out var message))
            return message;

        Log.A($"debug app-load {AppId}");
        return InsightsHtmlParts.PageStyles() + LogHtml.DumpTree($"2sxc load log for app {AppId}",
            appStates.Value.GetCacheState(AppId.Value).Log);
    }

}