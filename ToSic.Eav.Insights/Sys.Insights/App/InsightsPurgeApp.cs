using ToSic.Eav.Apps.Sys.Caching;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsPurgeApp(LazySvc<IAppCachePurge> appCachePurger, IAppsCatalog appsCatalog)
    : InsightsProvider(new()
    {
        Name = Link,
        Title = "Flush Logs"
    }, connect: [appCachePurger])
{
    public static string Link = "PurgeApp";

    public override string HtmlBody()
        => Purge(AppId);

    private string Purge(int? appId)
    {
        if (UrlParamsIncomplete(appId, out var message))
            return message;

        appCachePurger.Value.Purge(appsCatalog.AppIdentity(appId.Value));

        return $"app {appId} has been purged";
    }

}