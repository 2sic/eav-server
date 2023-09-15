namespace ToSic.Eav.WebApi.Sys.Insights
{
    public partial class InsightsControllerReal
    {
        private string Purge(int? appId)
        {
            if (UrlParamsIncomplete(appId, out var message))
                return message;

            SystemManager.PurgeApp(appId.Value);

            return $"app {appId} has been purged";
        }
    }
}
