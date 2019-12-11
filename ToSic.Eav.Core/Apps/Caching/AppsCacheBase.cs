using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Caching
{
    /// <summary>
    /// The Root Cache is the main cache for App States. It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
    /// This is just the abstract base implementation.
    /// The real cache must implement this and also provide platform specific adjustments so that the caching is in sync with the Environment.
    /// </summary>
    [PublicApi]
    // todo 2dm 2019-12-11 remove the ICacheKey from here
    public abstract class AppsCacheBase : HasLog,  /*IMetadataSource,*/ IAppsCache /*, IInAppAndZone*/ /*ICacheKey*/ /*ICanPurgeListCache*/
    {
        #region AppId ZoneId

        ///// <inheritdoc />
        //public virtual int AppId { get; set; }

        ///// <inheritdoc />
        //public virtual int ZoneId { get; set; }

        //public IAppsCache Init(int zoneId, int appId)
        //{
        //    //ZoneId = zoneId;
        //    //AppId = appId;
        //    return this;
        //}

        #endregion

        #region Constructors

        protected AppsCacheBase(string logName = "App.Cache", ILog parentLog = null) : base(logName, parentLog)
        {
        }


        #endregion

        /// <summary>
        /// The root / backend DataSource which can load apps as needed.
        /// </summary>
        private IAppsLoader Backend => _backend ?? (_backend = Factory.Resolve<IAppsLoader>());
	    private IAppsLoader _backend;

	    /// <summary>
	    /// Gets or sets the Dictionary of all Zones an Apps
	    /// </summary>
	    [PrivateApi("might rename this some day")]
	    public abstract Dictionary<int, Zone> ZoneApps { get; }

	    [PrivateApi("might rename this some day")]
        protected Dictionary<int, Zone> LoadZoneApps() => Backend.GetAllZones();

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
        public bool HasCacheItem(int zoneId, int appId) => HasCacheItem(string.Format(CacheKeySchema, zoneId, appId));

        /// <summary>
        /// Sets the CacheItem with specified CacheKey
        /// </summary>
        protected abstract void SetCacheItem(string cacheKey, AppState item);

		/// <summary>
		/// Get CacheItem with specified CacheKey
		/// </summary>
		protected abstract AppState GetCacheItem(string cacheKey);

		/// <summary>
		/// Remove the CacheItem with specified CacheKey
		/// </summary>
		protected abstract void RemoveCacheItem(string cacheKey);
        #endregion

  //      /// <summary>
		///// Ensure cache for current AppId
  //      /// In this case, the system will pick up the primary language from the surrounding context (e.g. HttpContext)
		///// </summary>
		//protected AppState EnsureCache() => EnsureCacheInternal(ZoneId, AppId);

        public AppState Get(int zoneId, int appId) => EnsureCacheInternal(zoneId, appId);

        /// <summary>
        /// Preload the cache with the given primary language
        /// Needed for cache buildup outside of a HttpContext (e.g. a Scheduler)
        /// </summary>
        /// <param name="primaryLanguage"></param>
        /// <returns></returns>
        public void PreLoadCache(int zoneId, int appId, string primaryLanguage) => EnsureCacheInternal(zoneId, appId, primaryLanguage);

        private AppState EnsureCacheInternal(int zoneId, int appId, string primaryLanguage = null)
        {
            if (zoneId == 0 || appId == 0)
                return null;

            var cacheKey = CachePartialKey(zoneId, appId);

            if (!HasCacheItem(cacheKey))
            {
                // create lock to prevent parallel initialization
                var lockKey = LoadLocks.GetOrAdd(cacheKey, new object());
                lock (lockKey)
                {
                    // now that lock is free, it could have been initialized, so re-check
                    if (!HasCacheItem(cacheKey))
                    {
                        // Init EavSqlStore once
                        var zone = GetZoneAppInternal(zoneId, appId);
                        Backend.InitZoneApp(zone.Item1, zone.Item2);
                        SetCacheItem(cacheKey, Backend.GetDataForCache(primaryLanguage));
                    }
                }
            }

            return GetCacheItem(cacheKey);
        }

        private static readonly ConcurrentDictionary<string, object> LoadLocks 
            = new ConcurrentDictionary<string, object>();

     //   /// <summary>
     //   /// Get the <see cref="AppState"/> of this app from the cache.
     //   /// </summary>
     //   // todo 2dm 2019-12-11 use parameters with IDs
	    //public AppState AppState => EnsureCache();

        #region Purge Cache

        /// <inheritdoc />
        /// <summary>
        /// Clear Cache for specific Zone/App
        /// </summary>
        public void PurgeCache(int zoneId, int appId) => RemoveCacheItem(string.Format(CacheKeySchema, zoneId, appId));

	    /// <inheritdoc />
	    /// <summary>
	    /// Clear Zones/Apps List
	    /// </summary>
	    public abstract void PurgeGlobalCache();

        public abstract void PartialUpdate(IEnumerable<int> entities);

        #endregion

        #region Cache-Chain

        //   /// <inheritdoc />
        //   public long CacheTimestamp => AppState.CacheTimestamp;
        ///// <inheritdoc />
        //public bool CacheChanged(long newCacheTimeStamp) => AppState.CacheChanged(newCacheTimeStamp);

        ///// <inheritdoc />
        public string CachePartialKey(int zoneId, int appId) => string.Format(CacheKeySchema, zoneId, appId);
        //   private string _cachePartialKey;

        ///// <inheritdoc />
        //public string CacheFullKey => CachePartialKey;

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        [PrivateApi]
		public Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null)
        {
            //EnsureCacheInternal(zoneId, appId);
            return GetZoneAppInternal(zoneId, appId);
		}

        // todo: better name etc.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// remarks: must be internal, as it must run after Ensure Cache
		private Tuple<int, int> GetZoneAppInternal(int? zoneId, int? appId)
		{
			var resultZoneId = zoneId ?? (appId.HasValue
			                       ? ZoneApps.Single(z => z.Value.Apps.Any(a => a.Key == appId.Value)).Key
			                       : Constants.DefaultZoneId);

			var resultAppId = appId.HasValue
								  ? ZoneApps[resultZoneId].Apps.Single(a => a.Key == appId.Value).Key
								  : ZoneApps[resultZoneId].DefaultAppId;

			return Tuple.Create(resultZoneId, resultAppId);
		}


     //   #region GetAssignedEntities by Guid, string and int

	    ///// <inheritdoc />
     //   public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null) 
     //       => AppState.Get(targetType, key, contentTypeName);

	    //#endregion


     //   #region Additional Stream Caching

	    ///// <inheritdoc />
     //   public IListCache Lists => _listsCache ?? (_listsCache = new ListCache(Log));
	    //private IListCache _listsCache;

     //   #endregion

    }
}
