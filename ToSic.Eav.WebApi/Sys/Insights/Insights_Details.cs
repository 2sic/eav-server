using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    public partial class InsightsControllerReal
    {
        /// <summary>
        /// WIP
        /// Trying to simplify access to all the features of Insights
        /// Using a single API endpoint which Dnn/Oqtane etc. must implement
        /// This to ensure that the Insights-Endpoint doesn't need changes to support more commands
        /// </summary>
        /// <returns></returns>
        public string Details(string view, int? appId, string key, int? position, string type, bool? toggle, string nameId, string filter)
        {
            // This is really important
            ThrowIfNotSystemAdmin();

            view = view.ToLowerInvariant();

            if (view.EqualsInsensitive(nameof(Help))) return Help();

            if (view.EqualsInsensitive(nameof(Licenses))) return Licenses();

            if (view.EqualsInsensitive(nameof(IsAlive))) return IsAlive();

            if (view.EqualsInsensitive(nameof(GlobalTypes))) return GlobalTypes();
            if (view.EqualsInsensitive(nameof(GlobalTypesLog))) return GlobalTypesLog();

            if (view.EqualsInsensitive(nameof(Logs)))
                return key == null
                    ? Logs()
                    : position == null
                        ? Logs(key, filter)
                        : Logs(key, position.Value);

            if (view.EqualsInsensitive(nameof(LogsFlush))) return LogsFlush(key);
            if (view.EqualsInsensitive(nameof(PauseLogs))) return PauseLogs(toggle ?? true);

            // Cache and Cache-Details
            if (view.EqualsInsensitive(nameof(Cache))) return Cache();
            if (view.EqualsInsensitive(nameof(LoadLog))) return LoadLog(appId);
            if (view.EqualsInsensitive(nameof(Stats))) return Stats(appId);
            if (view.EqualsInsensitive(nameof(Types))) return Types(appId);
            if (view.EqualsInsensitive(nameof(Purge))) return Purge(appId);

            // DataSourceCache
            if (view.EqualsInsensitive(nameof(_dsCache.Value.DataSourceCache))) return _dsCache.Value.DataSourceCache();
            if (view.EqualsInsensitive(nameof(_dsCache.Value.DataSourceCacheItem))) return _dsCache.Value.DataSourceCacheItem(key);
            if (view.EqualsInsensitive(nameof(_dsCache.Value.DataSourceCacheFlush))) return _dsCache.Value.DataSourceCacheFlush(key);
            if (view.EqualsInsensitive(nameof(_dsCache.Value.DataSourceCacheFlushAll))) return _dsCache.Value.DataSourceCacheFlushAll();

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
            if (view.EqualsInsensitive(nameof(LightSpeedStats))) return LightSpeedStats();

            return $"Error: View name {view} unknown";
        }
    }
}
