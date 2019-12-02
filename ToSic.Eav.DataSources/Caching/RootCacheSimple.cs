using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Simple, quick cache using static variables to store the cache. It's platform independent and very simple. 
    /// </summary>
    [PublicApi]
    public class RootCacheSimple : RootCacheBase
    {
        [PrivateApi]
        public override string LogId => "DS.QCache";

        [PrivateApi]
        public RootCacheSimple()
        {
            Cache = this;
        }

        [PrivateApi]
        public override Dictionary<int, Zone> ZoneApps
        {
            get
            {
                // ensure it's only loaded once, even if multiple threads are trying this at the same time
                if (_zoneAppsCache != null) return _zoneAppsCache;
                lock (ZoneAppLoadLock)
                    if (_zoneAppsCache == null)
                        _zoneAppsCache = LoadZoneApps();
                return _zoneAppsCache;
            }
        }

        private static Dictionary<int, Zone> _zoneAppsCache;
        private static readonly object ZoneAppLoadLock = new object();

        public override void PurgeGlobalCache() => _zoneAppsCache = null;

        /// <inheritdoc />
        public override string CacheKeySchema => "Z{0}A{1}";

        #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove
        private static readonly IDictionary<string, AppState> Caches = new Dictionary<string, AppState>();


        /// <inheritdoc />
        protected override bool HasCacheItem(string cacheKey) => Caches.ContainsKey(cacheKey);

        /// <inheritdoc />
        protected override void SetCacheItem(string cacheKey, AppState item)
        {
            try
            {
                // add or create
                // 2018-03-28 added lock - because I assume that's the cause of the random errors sometimes on system-load - see #1498
                lock (Caches)
                {
                    Caches[cacheKey] = item;
                }
            }
            catch (Exception ex)
            {
                // unclear why this pops up sometime...if it would also hit on live, so I'm adding some more info
                throw new Exception("issue with setting cache item - key is '" + cacheKey + "' and cache is null =" +
                                    (Caches == null) + " and item is null=" + (item == null), ex);
            }
        }

        [PrivateApi]
        protected override AppState GetCacheItem(string cacheKey) => Caches[cacheKey];

        [PrivateApi]
        protected override void RemoveCacheItem(string cacheKey) => Caches.Remove(cacheKey);    // returns false if key was not found (no Exception)

        #endregion


    }
}