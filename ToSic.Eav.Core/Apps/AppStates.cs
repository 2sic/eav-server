using System;
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

        public AppStates(IAppsCache appsCache, IServiceProvider serviceProvider)
        {
            Cache = appsCache;
            _serviceProvider = serviceProvider;
        }

        internal readonly IAppsCache Cache;
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => Cache.Get(_serviceProvider, app);

        /// <inheritdoc />
        public AppState Get(int appId) => Cache.Get(_serviceProvider, IdentityOfApp(appId));

        public IAppIdentity IdentityOfApp(int appId) =>
            new AppIdentity(Cache.ZoneIdOfApp(_serviceProvider, appId), appId);

        public IAppIdentity IdentityOfPrimary(int zoneId) => new AppIdentity(zoneId, PrimaryAppId(zoneId));

        public IAppIdentity IdentityOfDefault(int zoneId) => new AppIdentity(zoneId, DefaultAppId(zoneId));

        public string AppIdentifier(int zoneId, int appId) => Cache.Zones(_serviceProvider)[zoneId].Apps[appId];

        public int DefaultAppId(int zoneId) => Cache.Zones(_serviceProvider)[zoneId].DefaultAppId;

        public int PrimaryAppId(int zoneId) => Cache.Zones(_serviceProvider)[zoneId].PrimaryAppId;

        public IDictionary<int, string> Apps(int zoneId) => Cache.Zones(_serviceProvider)[zoneId].Apps;

        public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false)
        {
            var zone = Cache.Zones(_serviceProvider)[zoneId];
            return includeInactive ? zone.Languages : zone.Languages.Where(l => l.Active).ToList();
        }

        public IReadOnlyDictionary<int, Zone> Zones => Cache.Zones(_serviceProvider);
    }
}
