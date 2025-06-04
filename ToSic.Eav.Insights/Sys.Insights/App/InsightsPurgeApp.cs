using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.Apps.Sys.Caching;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsPurgeApp(LazySvc<AppCachePurger> appCachePurger) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appCachePurger])
{
    public static string Link = "PurgeApp";

    public override string Title => "Flush Logs";

    public override string HtmlBody()
        => Purge(AppId);

    private string Purge(int? appId)
    {
        if (UrlParamsIncomplete(appId, out var message))
            return message;

        appCachePurger.Value.PurgeApp(appId.Value);

        return $"app {appId} has been purged";
    }

}