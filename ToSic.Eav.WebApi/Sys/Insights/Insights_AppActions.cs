namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{
    private string Purge(int? appId)
    {
        if (UrlParamsIncomplete(appId, out var message))
            return message;

        AppCachePurger.PurgeApp(appId.Value);

        return $"app {appId} has been purged";
    }
}