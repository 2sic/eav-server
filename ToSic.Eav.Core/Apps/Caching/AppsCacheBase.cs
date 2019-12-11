using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Apps.Caching
{
    /// <summary>
    /// The Root Cache is the main cache for App States. It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
    /// This is just the abstract base implementation.
    /// The real cache must implement this and also provide platform specific adjustments so that the caching is in sync with the Environment.
    /// </summary>
    [PublicApi]
    public abstract class AppsCacheBase : IAppsCache
    {
        /// <summary>
        /// The repository loader. Must generate a new one on every access, to be sure that it doesn't stay in memory for long. 
        /// </summary>
        private IRepositoryLoader GetNewRepoLoader() => Factory.Resolve<IRepositoryLoader>();

	    /// <summary>
	    /// Gets or sets the Dictionary of all Zones an Apps
	    /// </summary>
	    [PrivateApi("might rename this some day")]
	    public abstract Dictionary<int, Zone> ZoneApps { get; }

        [PrivateApi("might rename this some day")]
        protected Dictionary<int, Zone> LoadZoneApps() => GetNewRepoLoader().Zones();

	    /// <summary>
		/// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
		/// </summary>
		[PrivateApi]
		public abstract string CacheKeySchema { get; }


	    #region Definition of the abstract Has-Item, Set, Get, Remove
        /// <summary>
		/// Test whether CacheKey exists in the Cache
		/// </summary>
		protected abstract bool HasCacheItem(string cacheKey);

        /// <summary>
        /// Check if an app is already in the global cache.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool Has(int zoneId, int appId) => HasCacheItem(string.Format(CacheKeySchema, zoneId, appId));

        /// <summary>
        /// Sets the CacheItem with specified CacheKey
        /// </summary>
        protected abstract void Set(string cacheKey, AppState item);

		/// <summary>
		/// Get CacheItem with specified CacheKey
		/// </summary>
		protected abstract AppState Get(string cacheKey);

		/// <summary>
		/// Remove the CacheItem with specified CacheKey
		/// </summary>
		protected abstract void Remove(string cacheKey);
        #endregion

        public AppState Get(int zoneId, int appId) => EnsureCacheInternal(zoneId, appId);

        /// <summary>
        /// Preload the cache with the given primary language
        /// Needed for cache buildup outside of a HttpContext (e.g. a Scheduler)
        /// </summary>
        public void ForceLoad(int zoneId, int appId, string primaryLanguage) => EnsureCacheInternal(zoneId, appId, primaryLanguage);

        private AppState EnsureCacheInternal(int zoneId, int appId, string primaryLanguage = null)
        {
            if (zoneId == 0 || appId == 0)
                return null;

            var cacheKey = CacheKey(zoneId, appId);

            if (HasCacheItem(cacheKey)) return Get(cacheKey);

            // create lock to prevent parallel initialization
            var lockKey = LoadLocks.GetOrAdd(cacheKey, new object());
            lock (lockKey)
            {
                // now that lock is free, it could have been initialized, so re-check
                if (HasCacheItem(cacheKey)) return Get(cacheKey);

                // Init EavSqlStore once
                var identity = GetZoneAppInternal(zoneId, appId);
                var loader = GetNewRepoLoader();
                if (primaryLanguage != null) loader.PrimaryLanguage = primaryLanguage;
                var appState = loader.AppPackage(identity.AppId, null);

                Set(cacheKey, appState);
            }

            return Get(cacheKey);
        }

        private static readonly ConcurrentDictionary<string, object> LoadLocks 
            = new ConcurrentDictionary<string, object>();

        #region Purge Cache

        /// <inheritdoc />
        /// <summary>
        /// Clear Cache for specific Zone/App
        /// </summary>
        public void PurgeCache(int zoneId, int appId) => Remove(string.Format(CacheKeySchema, zoneId, appId));

	    /// <inheritdoc />
	    /// <summary>
	    /// Clear Zones/Apps List
	    /// </summary>
	    public abstract void PurgeGlobalCache();

        public abstract void PartialUpdate(IInAppAndZone app, IEnumerable<int> entities, ILog log);

        #endregion

        #region Cache-Chain

        ///// <inheritdoc />
        public string CacheKey(int zoneId, int appId) => string.Format(CacheKeySchema, zoneId, appId);

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        [PrivateApi]
		public IInAppAndZone GetIdentity(int? zoneId = null, int? appId = null) 
            => GetZoneAppInternal(zoneId, appId);

        // todo: better name etc.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// remarks: must be internal, as it must run after Ensure Cache
		private IInAppAndZone GetZoneAppInternal(int? zoneId, int? appId)
		{
			var resultZoneId = zoneId ?? (appId.HasValue
			                       ? ZoneApps.Single(z => z.Value.Apps.Any(a => a.Key == appId.Value)).Key
			                       : Constants.DefaultZoneId);

			var resultAppId = appId.HasValue
								  ? ZoneApps[resultZoneId].Apps.Single(a => a.Key == appId.Value).Key
								  : ZoneApps[resultZoneId].DefaultAppId;

			return new AppZoneIds(resultZoneId, resultAppId);
        }

    }
}
