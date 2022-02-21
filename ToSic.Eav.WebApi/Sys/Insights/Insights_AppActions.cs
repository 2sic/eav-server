﻿namespace ToSic.Eav.WebApi.Sys
{
    public partial class Insights
    {
        private string Purge(int? appId)
        {
            if (UrlParamsIncomplete(appId, out var message))
                return message;

            SystemManager.Init(Log).PurgeApp(appId.Value);

            return $"app {appId} has been purged";
        }
    }
}
