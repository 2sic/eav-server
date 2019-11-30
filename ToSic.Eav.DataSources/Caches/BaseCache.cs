using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using AppState = ToSic.Eav.Apps.AppState;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caches
{
    /// <inheritdoc cref="DataSourceBase" />
    /// <summary>
    /// Represents an abstract Cache DataSource
    /// </summary>
    public abstract class BaseCache : DataSourceBase, IMetadataSource, ICache
	{

        protected new BaseCache Cache { get; set; }

		protected BaseCache()
		{
		    // ReSharper disable VirtualMemberCallInConstructor
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, () => AppState.List));
			Out.Add(Constants.PublishedStreamName, new DataStream(this, Constants.PublishedStreamName, () => AppState.ListPublished));
			Out.Add(Constants.DraftsStreamName, new DataStream(this, Constants.DraftsStreamName, () => AppState.ListNotHavingDrafts));
		    // ReSharper restore VirtualMemberCallInConstructor

            Lists.ListDefaultRetentionTimeInSeconds = 60 * 60;
		}

        /// <summary>
		/// The root DataSource
		/// </summary>
		private IRootSource Backend => _backend ?? (_backend = Factory.Resolve<IRootSource>());
	    private IRootSource _backend;

	    /// <summary>
	    /// Gets or sets the Dictionary of all Zones an Apps
	    /// </summary>
	    public abstract Dictionary<int, Zone> ZoneApps { get; }

        protected Dictionary<int, Zone> LoadZoneApps() => Backend.GetAllZones();

	    /// <summary>
		/// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
		/// </summary>
		public abstract string CacheKeySchema { get; }


	    #region Definition of the abstract Has-Item, Set, Get, Remove
        /// <summary>
		/// Test whether CacheKey exists in Cache
		/// </summary>
		protected abstract bool HasCacheItem(string cacheKey);

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

        /// <summary>
		/// Ensure cache for current AppId
        /// In this case, the system will pick up the primary language from the surrounding context (e.g. HttpContext)
		/// </summary>
		protected AppState EnsureCache()
		{
            return EnsureCacheInternal();
        }

        /// <summary>
        /// Preload the cache with the given primary language
        /// Needed for cache buildup outside of a HttpContext (e.g. a Scheduler)
        /// </summary>
        /// <param name="primaryLanguage"></param>
        /// <returns></returns>
        public void PreLoadCache(string primaryLanguage)
        {
            EnsureCacheInternal(primaryLanguage);
        }

        private AppState EnsureCacheInternal(string primaryLanguage = null)
        {
            if (ZoneId == 0 || AppId == 0)
                return null;

            var cacheKey = CachePartialKey;

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
                        var zone = GetZoneAppInternal(ZoneId, AppId);
                        Backend.InitZoneApp(zone.Item1, zone.Item2);
                        SetCacheItem(cacheKey, Backend.GetDataForCache(primaryLanguage));
                    }
                }
            }

            return GetCacheItem(cacheKey);
        }

        private static readonly ConcurrentDictionary<string, object> LoadLocks 
            = new ConcurrentDictionary<string, object>();

	    public AppState AppState => EnsureCache();

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

	    #region Cache-Chain

	    public override long CacheTimestamp => AppState.CacheTimestamp;
	    public override bool CacheChanged(long newCacheTimeStamp) => AppState.CacheChanged(newCacheTimeStamp);

        private string _cachePartialKey;
	    public override string CachePartialKey
	    {
            get
            {
                if (string.IsNullOrEmpty(_cachePartialKey))
                    _cachePartialKey = string.Format(CacheKeySchema, ZoneId, AppId);
                return _cachePartialKey;
            }
	    }

	    public override string CacheFullKey => CachePartialKey;

	    #endregion

	    /// <inheritdoc />
	    /// <summary>
	    /// Get a ContentType by StaticName if found of DisplayName if not
	    /// </summary>
	    /// <param name="name">Either StaticName or DisplayName</param>
	    /// <returns>a content-type OR null</returns>
	    public IContentType GetContentType(string name) => AppState.GetContentType(name);

		/// <inheritdoc />
		/// <summary>
		/// Get a ContentType by Id
		/// </summary>
		public IContentType GetContentType(int contentTypeId) => AppState.GetContentType(contentTypeId);

	    /// <summary>
		/// Get all Content Types
		/// </summary>
		public IEnumerable<IContentType> GetContentTypes() => AppState.ContentTypes;

	    /// <inheritdoc />
	    /// <summary>
	    /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
	    /// </summary>
	    /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		public Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null)
		{
			EnsureCache();
			return GetZoneAppInternal(zoneId, appId);
		}

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


        #region GetAssignedEntities by Guid, string and int

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null) 
            => AppState.Get(targetType, key, contentTypeName);

	    #endregion


        #region Additional Stream Caching

        public IListsCache Lists => _listsCache ?? (_listsCache = new ListsCache());
	    private IListsCache _listsCache;

        #endregion

        public override void InitLog(string name, ILog parentLog = null, string initialMessage = null)
	    {
	        // ignore
	    }
	}
}
