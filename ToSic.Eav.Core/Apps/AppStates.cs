﻿using System;
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
            _cache = appsCache;
            _serviceProvider = serviceProvider;
        }

        private readonly IAppsCache _cache;
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => _cache.Get(_serviceProvider, app);

        /// <inheritdoc />
        public AppState Get(int appId) => _cache.Get(_serviceProvider, appId);

        public IAppIdentity Identity(int? zoneId, int? appId) => _cache.GetIdentity(_serviceProvider, zoneId, appId);

        public string AppIdentifier(int zoneId, int appId) => _cache.Zones(_serviceProvider)[zoneId].Apps[appId];

        public int DefaultAppId(int zoneId) => _cache.Zones(_serviceProvider)[zoneId].DefaultAppId;

        public IDictionary<int, string> Apps(int zoneId) => _cache.Zones(_serviceProvider)[zoneId].Apps;

        public List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false)
        {
            var zone = _cache.Zones(_serviceProvider)[zoneId];
            return includeInactive ? zone.Languages : zone.Languages.Where(l => l.Active).ToList();
        }

        public IReadOnlyDictionary<int, Zone> Zones => _cache.Zones(_serviceProvider);
    }
}
