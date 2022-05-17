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

        public AppStates(IServiceProvider serviceProvider, AppsCacheSwitch appsCacheSwitch)
        {
            _serviceProvider = serviceProvider;
            AppsCacheSwitch = appsCacheSwitch;
        }

        private readonly IServiceProvider _serviceProvider;
        internal readonly AppsCacheSwitch AppsCacheSwitch;

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => AppsCacheSwitch.Value.Get(_serviceProvider, app);

        /// <inheritdoc />
        public AppState Get(int appId) => AppsCacheSwitch.Value.Get(_serviceProvider, IdentityOfApp(appId));

        public bool IsCached(IAppIdentity appId) => AppsCacheSwitch.Value.Has(appId);

        public IAppIdentity IdentityOfApp(int appId) =>
            new AppIdentity(AppsCacheSwitch.Value.ZoneIdOfApp(_serviceProvider, appId), appId);

        public IAppIdentity IdentityOfPrimary(int zoneId) => new AppIdentity(zoneId, PrimaryAppId(zoneId));

        public IAppIdentity IdentityOfDefault(int zoneId) => new AppIdentity(zoneId, DefaultAppId(zoneId));

        public string AppIdentifier(int zoneId, int appId) => AppsCacheSwitch.Value.Zones(_serviceProvider)[zoneId].Apps[appId];

        public int DefaultAppId(int zoneId) => AppsCacheSwitch.Value.Zones(_serviceProvider)[zoneId].DefaultAppId;

        public int PrimaryAppId(int zoneId) => AppsCacheSwitch.Value.Zones(_serviceProvider)[zoneId].PrimaryAppId;

        public IDictionary<int, string> Apps(int zoneId) => AppsCacheSwitch.Value.Zones(_serviceProvider)[zoneId].Apps;

        public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false)
        {
            var zone = AppsCacheSwitch.Value.Zones(_serviceProvider)[zoneId];
            return includeInactive ? zone.Languages : zone.Languages.Where(l => l.Active).ToList();
        }

        public IReadOnlyDictionary<int, Zone> Zones => AppsCacheSwitch.Value.Zones(_serviceProvider);
    }
}
