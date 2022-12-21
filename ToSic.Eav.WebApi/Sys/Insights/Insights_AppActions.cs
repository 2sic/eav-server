using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi.Sys
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
