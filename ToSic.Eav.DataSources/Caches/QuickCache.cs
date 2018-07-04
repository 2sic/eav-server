using System;
using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources.Caches
{
    /// <inheritdoc />
    /// <summary>
    /// simple, quick cache using static variables to store the cache
    /// </summary>
    public class QuickCache : BaseCache
    {
        public override string LogId => "DS.QCache";

        public QuickCache()
        {
            Cache = this;
        }

        public override Dictionary<int, Zone> ZoneApps 
            => _zoneAppsCache ?? (_zoneAppsCache = LoadZoneApps());
        private static Dictionary<int, Zone> _zoneAppsCache;

        public override void PurgeGlobalCache() => _zoneAppsCache = null;

        public override string CacheKeySchema => "Z{0}A{1}";

        #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove
        private static readonly IDictionary<string, AppDataPackage> Caches = new Dictionary<string, AppDataPackage>();


        protected override bool HasCacheItem(string cacheKey) => Caches.ContainsKey(cacheKey);

        protected override void SetCacheItem(string cacheKey, AppDataPackage item)
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

        protected override AppDataPackage GetCacheItem(string cacheKey) => Caches[cacheKey];

        protected override void RemoveCacheItem(string cacheKey) => Caches.Remove(cacheKey);    // returns false if key was not found (no Exception)

        #endregion


    }
}