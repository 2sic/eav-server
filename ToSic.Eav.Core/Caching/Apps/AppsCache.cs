using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Caching.Apps
{
    public class AppsCache: AppsCacheBase
    {
        #region Constructor

        // It's important that
        // - this object has no constructor (or an empty one) because of DI
        // - that it doesn't keep a log or similar inside it, because this object can sometimes be static-cached forever. 
        //public AppsCache(): base("App.Cache", null) { }

        #endregion

        [PrivateApi]
        public override Dictionary<int, Zone> Zones
        {
            get
            {
                // ensure it's only loaded once, even if multiple threads are trying this at the same time
                if (ZoneAppCache != null) return ZoneAppCache;
                lock (ZoneAppLoadLock)
                    if (ZoneAppCache == null)
                        ZoneAppCache = LoadZoneApps();
                return ZoneAppCache;
            }
        }
        protected static volatile Dictionary<int, Zone> ZoneAppCache;
        protected static readonly object ZoneAppLoadLock = new object();


        #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove
        private static readonly IDictionary<string, AppState> Caches = new Dictionary<string, AppState>();


        /// <inheritdoc />
        protected override bool Has(string cacheKey) => Caches.ContainsKey(cacheKey);

        /// <inheritdoc />
        protected override void Set(string cacheKey, AppState item)
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

        /// <inheritdoc />
        protected override AppState Get(string cacheKey) => Caches[cacheKey];

        /// <inheritdoc />
        protected override void Remove(string cacheKey) => Caches.Remove(cacheKey);    // returns false if key was not found (no Exception)

        #endregion

        /// <inheritdoc />
        public override void PurgeAll() => ZoneAppCache = null;

        ///// <inheritdoc />
        //public override void Update(IAppIdentity app, IEnumerable<int> entities, ILog log) => base.Update(app, entities, log);
    }
}
