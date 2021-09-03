using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This is the implementation of States which doesn't use the static Eav.Apps.State
    /// It's not final, so please keep very internal
    /// The names of the Get etc. will probably change a few more times
    /// </summary>
    /// <remarks>
    /// Important: this can be a long-living object, so never use loggers or anything here. 
    /// </remarks>
    [PrivateApi("internal")]
    public class AppStates: IAppStates
    {

        public AppStates(IAppsCache appsCache) => Cache = appsCache;
        private readonly IAppsCache Cache;

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => Cache.Get(app);

        /// <inheritdoc />
        public AppState Get(int appId) => Cache.Get(appId);

        public IAppIdentity Identity(int? zoneId, int? appId) => Cache.GetIdentity(zoneId, appId);

        public string AppIdentifier(int zoneId, int appId) => Cache.Zones[zoneId].Apps[appId];

        public int DefaultAppId(int zoneId) => Cache.Zones[zoneId].DefaultAppId;

        public IDictionary<int, string> Apps(int zoneId) => Cache.Zones[zoneId].Apps;

        public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false) => includeInactive
            ? Cache.Zones[zoneId].Languages
            : Cache.Zones[zoneId].Languages.Where(l => l.Active).ToList();

        public IReadOnlyDictionary<int, Zone> Zones => Cache.Zones;
    }
}
