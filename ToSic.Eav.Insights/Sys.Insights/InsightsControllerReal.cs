﻿using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Razor.Blade;
using ToSic.Sys.Users;
using static System.StringComparer;

namespace ToSic.Eav.Sys.Insights;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class InsightsControllerReal(IUser user, LazySvc<InsightsDataSourceCache> dsCache, IEnumerable<IInsightsProvider> insightsProviders, IHttpExceptionMaker exceptionMaker)
    : ServiceBase("Api.SysIns", connect: [user, dsCache, insightsProviders])
{
    public const string LogSuffix = "Insight";

    /// <summary>
    /// WIP
    /// Trying to simplify access to all the features of Insights
    /// Using a single API endpoint which Dnn/Oqtane etc. must implement
    /// This to ensure that the Insights-Endpoint doesn't need changes to support more commands
    /// </summary>
    /// <returns></returns>
    public string Details(string view, int? appId, string key, int? position, string type, bool? toggle, string nameId, string filter)
    {
        var l = Log.Fn<string>($"view:{view}, appId:{appId}, key:{key}, position:{position}, type:{type}, toggle:{toggle}, nameId:{nameId}, filter:{filter}");
        // This is really important
        ThrowIfNotSystemAdmin();

        view = view.ToLowerInvariant();

        var provider = insightsProviders.FirstOrDefault(p => p.Specs.Name.EqualsInsensitive(view));
        if (provider != null)
        {
            l.A($"found provider {provider.Specs.Name}");
            provider.SetContext(new InsightsHtmlTable(), appId, new Dictionary<string, object?>(InvariantCultureIgnoreCase)
            {
                {"key", key},
                {"position", position},
                {"type", type},
                {"toggle", toggle},
                {"nameId", nameId},
                {"filter", filter}
            }, key, position, type, toggle, nameId, filter);
            var result = provider.HtmlBody();

            var wrapper = Tag.Custom("html",
                Tag.Head(
                    Tag.Custom("title", $"Insights: {provider.Specs.Name}")
                ),
                "\n",
                Tag.H1(provider.Specs.Title.Replace("[AppId]", appId?.ToString() ?? "unknown")),
                "\n",
                Tag.Custom("body",
                    result
                )
            );
            return l.ReturnAsOk(wrapper.ToString());
        }

        // DataSourceCache
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCache))) return dsCache.Value.DataSourceCache();
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheItem))) return dsCache.Value.DataSourceCacheItem(key);
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheFlush))) return dsCache.Value.DataSourceCacheFlush(key);
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheFlushAll))) return dsCache.Value.DataSourceCacheFlushAll();

        return $"Error: View name {view} unknown";
    }

    private void ThrowIfNotSystemAdmin()
    {
        if (!user.IsSystemAdmin) throw exceptionMaker.PermissionDenied("requires Superuser permissions");
    }
    
}