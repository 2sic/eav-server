using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Practices.Unity;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Represents an abstract Cache DataSource
	/// </summary>
	public abstract class BaseCache : BaseDataSource, IMetaDataSource, ICache, IDeferredEntitiesList
	{
		protected new BaseCache Cache { get; set; }

		protected BaseCache()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities, GetList));
			Out.Add(Constants.PublishedStreamName, new DataStream(this, Constants.PublishedStreamName, GetPublishedEntities));
			Out.Add(Constants.DraftsStreamName, new DataStream(this, Constants.DraftsStreamName, GetDraftEntities));

            ListDefaultRetentionTimeInSeconds = 60 * 60;
		}

        #region Default Streams: All=Default; Published, Draft
        private IDictionary<int, IEntity> GetEntities() => EnsureCache().Entities;

	    /// <summary>
        /// This retrieves the cached list-only set (without the dictionar)
        /// </summary>
        /// <returns></returns>
	    private IEnumerable<IEntity> GetList() => EnsureCache().List;

	    private IDictionary<int, IEntity> GetPublishedEntities() => EnsureCache().PublishedEntities;

	    private IDictionary<int, IEntity> GetDraftEntities() => EnsureCache().DraftEntities;

	    #endregion

        /// <summary>
		/// The root DataSource
		/// </summary>
		/// <remarks>Unity sets this automatically</remarks>
		public IRootSource Backend => _backend ?? (_backend = Factory.Resolve<IRootSource>());
	    private IRootSource _backend;

		/// <summary>
		/// Gets or sets the Dictionary of all Zones an Apps
		/// </summary>
		public abstract Dictionary<int, Zone> ZoneApps { get; protected set; }

		/// <summary>
		/// Gets or sets the Dictionary of all AssignmentObjectTypes
		/// </summary>
		public abstract Dictionary<int, string> AssignmentObjectTypes { get; protected set; }

		/// <summary>
		/// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
		/// </summary>
		public abstract string CacheKeySchema { get; }

		/// <summary>
		/// Gets the DateTime when this Cache was populated
		/// </summary>
		public DateTime LastRefresh => EnsureCache().LastRefresh;

	    #region Definition of the abstract Has-Item, Set, Get, Remove
        /// <summary>
		/// Test whether CacheKey exists in Cache
		/// </summary>
		protected abstract bool HasCacheItem(string cacheKey);
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
		/// </summary>
		protected AppDataPackage EnsureCache()
		{
			if (ZoneApps == null || AssignmentObjectTypes == null)
			{
				ZoneApps = Backend.GetAllZones();

				AssignmentObjectTypes = Backend.GetAssignmentObjectTypes();
			}

			if (ZoneId == 0 || AppId == 0)
				return null;

            var cacheKey = CachePartialKey;

			if (!HasCacheItem(cacheKey))
			{
				// Init EavSqlStore once
				var zone = GetZoneAppInternal(ZoneId, AppId);
				Backend.InitZoneApp(zone.Item1, zone.Item2);

				SetCacheItem(cacheKey, Backend.GetDataForCache(Cache));
			}

			return GetCacheItem(cacheKey);
		}

		/// <summary>
		/// Clear Cache for specific Zone/App
		/// </summary>
		public void PurgeCache(int zoneId, int appId)
		{
			var cacheKey = string.Format(CacheKeySchema, zoneId, appId);

			RemoveCacheItem(cacheKey);
		}

		/// <summary>
		/// Clear Zones/Apps List
		/// </summary>
		public void PurgeGlobalCache()
		{
			ZoneApps = null;
		}

        #region Cache-Chain

	    public override DateTime CacheLastRefresh => EnsureCache().LastRefresh;

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

        /// <summary>
		/// Get a ContentType by StaticName if found of DisplayName if not
		/// </summary>
		/// <param name="name">Either StaticName or DisplayName</param>
		public IContentType GetContentType(string name)
		{
			var cache = EnsureCache();
			// Lookup StaticName first
			var matchByStaticName = cache.ContentTypes.FirstOrDefault(c => c.Value.StaticName.Equals(name));
			if (matchByStaticName.Value != null)
				return matchByStaticName.Value;

			// Lookup Name afterward
			var matchByName = cache.ContentTypes.FirstOrDefault(c => c.Value.Name.Equals(name));
		    return matchByName.Value;
		}

		/// <summary>
		/// Get a ContentType by Id
		/// </summary>
		public IContentType GetContentType(int contentTypeId) => EnsureCache().ContentTypes.FirstOrDefault(c => c.Key == contentTypeId).Value;

	    /// <summary>
		/// Get all Content Types
		/// </summary>
		public IDictionary<int, IContentType> GetContentTypes() => EnsureCache().ContentTypes;

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

		/// <summary>
		/// Get AssignmentObjectTypeId by Name
		/// </summary>
		public int GetAssignmentObjectTypeId(string assignmentObjectTypeName)
		{
			EnsureCache();

			return AssignmentObjectTypes.SingleOrDefault(a => a.Value == assignmentObjectTypeName).Key;
		}

        #region GetAssignedEntities by Guid, string and int
        /// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, Guid key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<Guid, IEnumerable<IEntity>> keyGuidDictionary;
			if (cache.AssignmentObjectTypesGuid.TryGetValue(assignmentObjectTypeId, out keyGuidDictionary))
			{ 
				IEnumerable<IEntity> entities;
				if (keyGuidDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, string key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<string, IEnumerable<IEntity>> keyStringDictionary;
			if (cache.AssignmentObjectTypesString.TryGetValue(assignmentObjectTypeId, out keyStringDictionary))
			{
				IEnumerable<IEntity> entities;
				if (keyStringDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
		}

		/// <summary>
		/// Get Entities with specified AssignmentObjectTypeId and Key
		/// </summary>
		public IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, int key, string contentTypeName = null)
		{
			var cache = EnsureCache();

			Dictionary<int, IEnumerable<IEntity>> keyNumberDictionary;
			if (cache.AssignmentObjectTypesNumber.TryGetValue(assignmentObjectTypeId, out keyNumberDictionary))
			{
				IEnumerable<IEntity> entities;
				if (keyNumberDictionary.TryGetValue(key, out entities))
					return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
			}

			return new List<IEntity>();
        }
        #endregion


        #region Additional Stream Caching

        // todo: check what happens with this in a DNN environment; I guess it works, but there are risks...
        private ObjectCache ListCache => MemoryCache.Default;

	    #region Has List
        public bool ListHas(string key)
        {
            return ListCache.Contains(key);
        }

        public bool ListHas(IDataStream dataStream)
        {
            return ListHas(dataStream.Source.CacheFullKey + "|" + dataStream.Name);
        }
        #endregion

        public int ListDefaultRetentionTimeInSeconds { get; set; }

        #region Get List
        /// <summary>
        /// Get a DataStream in the cache - will be null if not found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ListCacheItem ListGet(string key)
        {
            var ds = ListCache[key] as ListCacheItem;
            return ds;
        }

        public ListCacheItem ListGet(IDataStream dataStream)
        {
            return ListGet(dataStream.Source.CacheFullKey + "|" + dataStream.Name);
        }
        #endregion

        #region Set/Add List

        /// <summary>
        /// Insert a data-stream to the cache - if it can be found
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="durationInSeconds"></param>
        public void ListSet(string key, IEnumerable<IEntity> list, DateTime sourceRefresh, int durationInSeconds = 0)
        {
            var policy = new CacheItemPolicy();
            policy.SlidingExpiration = new TimeSpan(0, 0, durationInSeconds > 0 ? durationInSeconds : ListDefaultRetentionTimeInSeconds);

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(key, list, sourceRefresh), policy);
        }

        public void ListSet(IDataStream dataStream, int durationInSeconds = 0)
        {
            ListSet(dataStream.Source.CacheFullKey + "|" + dataStream.Name, dataStream.LightList, dataStream.Source.CacheLastRefresh, durationInSeconds);
        }
        #endregion

        #region Remove List
        public void ListRemove(string key)
        {
            var cache = MemoryCache.Default;
            cache.Remove(key);
        }

        public void ListRemove(IDataStream dataStream)
        {
            ListRemove(dataStream.Source.CacheFullKey + "|" + dataStream.Name);
        }
        #endregion

        #endregion

        IDictionary<int, IEntity> IDeferredEntitiesList.List => Out[Constants.DefaultStreamName].List;

	    IEnumerable<IEntity> IDeferredEntitiesList.LightList => Out[Constants.DefaultStreamName].LightList;
	}
}
