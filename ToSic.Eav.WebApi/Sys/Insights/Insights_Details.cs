using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Razor.Blade;
using static System.StringComparer;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{
    private IInsightsProvider FindProvider(string name)
    {
        var provider = insightsProviders.FirstOrDefault(p => p.Name.EqualsInsensitive(name));
        return provider;
    }

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

        var provider = FindProvider(view);
        if (provider != null)
        {
            l.A($"found provider {provider.Name}");
            provider.SetContext(HtmlTableBuilder, appId, new Dictionary<string, object>(InvariantCultureIgnoreCase)
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
                    Tag.Custom("title", $"Insights: {provider.Name}")
                ),
                "\n",
                Tag.H1(provider.Title),
                "\n",
                Tag.Custom("body",
                    result
                )
            );
            return l.ReturnAsOk(wrapper.ToString());
        }

        if (view.EqualsInsensitive(nameof(Help))) return Help();

        if (view.EqualsInsensitive(nameof(Licenses))) return Licenses();

        if (view.EqualsInsensitive(nameof(GlobalTypesLog))) return GlobalTypesLog();

        // Cache and Cache-Details
        if (view.EqualsInsensitive(nameof(Cache))) return Cache();
        if (view.EqualsInsensitive(nameof(LoadLog))) return LoadLog(appId);
        if (view.EqualsInsensitive(nameof(Stats))) return Stats(appId);
        if (view.EqualsInsensitive(nameof(Purge))) return Purge(appId);

        // DataSourceCache
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCache))) return dsCache.Value.DataSourceCache();
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheItem))) return dsCache.Value.DataSourceCacheItem(key);
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheFlush))) return dsCache.Value.DataSourceCacheFlush(key);
        if (view.EqualsInsensitive(nameof(dsCache.Value.DataSourceCacheFlushAll))) return dsCache.Value.DataSourceCacheFlushAll();

        // Cache: Entities and Details, Attributes, Metadata etc.
        if (view.EqualsInsensitive(nameof(Entities))) return Entities(appId, type);
        if (view.EqualsInsensitive(nameof(Entity))) return Entity(appId, nameId);
        if (view.EqualsInsensitive(nameof(EntityMetadata))) return EntityMetadata(appId, int.Parse(nameId));
        if (view.EqualsInsensitive(nameof(EntityPermissions))) return EntityPermissions(appId, int.Parse(nameId));

        if (view.EqualsInsensitive(nameof(Attributes))) return Attributes(appId, type);
        if (view.EqualsInsensitive(nameof(AttributeMetadata))) return AttributeMetadata(appId, type, nameId);
        if (view.EqualsInsensitive(nameof(AttributePermissions))) return AttributePermissions(appId, type, nameId);
        if (view.EqualsInsensitive(nameof(TypeMetadata))) return TypeMetadata(appId, type);
        if (view.EqualsInsensitive(nameof(TypePermissions))) return TypePermissions(appId, type);

        return $"Error: View name {view} unknown";
    }
}