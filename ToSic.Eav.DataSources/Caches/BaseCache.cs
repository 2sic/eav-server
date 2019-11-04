using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.DataSources.Caches
{
    /// <inheritdoc cref="BaseDataSource" />
    /// <summary>
    /// Represents an abstract Cache DataSource
    /// </summary>
    public abstract class BaseCache : BaseDataSource, IMetadataProvider, ICache
	{

        protected new BaseCache Cache { get; set; }

		protected BaseCache()
		{
		    // ReSharper disable VirtualMemberCallInConstructor
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, () => AppDataPackage.List));
			Out.Add(Constants.PublishedStreamName, new DataStream(this, Constants.PublishedStreamName, () => AppDataPackage.ListPublished));
			Out.Add(Constants.DraftsStreamName, new DataStream(this, Constants.DraftsStreamName, () => AppDataPackage.ListNotHavingDrafts));
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
        protected abstract void SetCacheItem(string cacheKey, AppDataPackage item);

		/// <summary>
		/// Get CacheItem with specified CacheKey
		/// </summary>
		protected abstract AppDataPackage GetCacheItem(string cacheKey);

		/// <summary>
		/// Remove the CacheItem with specified CacheKey
		/// </summary>
		protected abstract void RemoveCacheItem(string cacheKey);
        #endregion

        /// <summary>
		/// Ensure cache for current AppId
        /// In this case, the system will pick up the primary language from the surrounding context (e.g. HttpContext)
		/// </summary>
		protected AppDataPackage EnsureCache()
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

        private AppDataPackage EnsureCacheInternal(string primaryLanguage = null)
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

	    public AppDataPackage AppDataPackage => EnsureCache();

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

	    public override long CacheTimestamp => AppDataPackage.CacheTimestamp;
	    public override bool CacheChanged(long newCacheTimeStamp) => AppDataPackage.CacheChanged(newCacheTimeStamp);

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
	    public IContentType GetContentType(string name) => AppDataPackage.GetContentType(name);

		/// <inheritdoc />
		/// <summary>
		/// Get a ContentType by Id
		/// </summary>
		public IContentType GetContentType(int contentTypeId) => AppDataPackage.GetContentType(contentTypeId);

	    /// <summary>
		/// Get all Content Types
		/// </summary>
		public IEnumerable<IContentType> GetContentTypes() => AppDataPackage.ContentTypes;

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

        public IEnumerable<IEntity> GetMetadata<T>(int targetType, T key, string contentTypeName = null) 
            => AppDataPackage.GetMetadata(targetType, key, contentTypeName);

	    #endregion


        #region Additional Stream Caching

        public IListsCache Lists => _listsCache ?? (_listsCache = new ListsCache());
	    private IListsCache _listsCache;

        #endregion

        public override void InitLog(string name, Log parentLog = null, string initialMessage = null)
	    {
	        // ignore
	    }
	}
}
