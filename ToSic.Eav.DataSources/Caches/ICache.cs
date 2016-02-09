using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Caches
{
	/// <summary>
	/// Caching interface for standard Eav Cache
	/// </summary>
	[PipelineDesigner]
	public interface ICache : IDataSource
	{
		/// <summary>
		/// Clean cache for specific Zone and App
		/// </summary>
		void PurgeCache(int zoneId, int appId);

		/// <summary>
		/// Clean global cache (currently contains List of Zones and Apps)
		/// </summary>
		void PurgeGlobalCache();

		/// <summary>
		/// Gets the DateTime when this CacheItem was populated
		/// </summary>
		DateTime LastRefresh { get; }

		/// <summary>
		/// Gets a GeontentType by Name
		/// </summary>
		IContentType GetContentType(string name);
		/// <summary>
		/// Gets a ContentType by Id
		/// </summary>
		IContentType GetContentType(int contentTypeId);

		/// <summary>
		/// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

		/// <summary>
		/// Get AssignmentObjectTypeId by Name
		/// </summary>
		int GetAssignmentObjectTypeId(string assignmentObjectTypeName);


        #region Interfaces for the List-Cache

        /// <summary>
        /// The time a list stays in the cache by default - usually 3600 = 1 hour
        /// </summary>
        int ListDefaultRetentionTimeInSeconds { get; set; }

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ListCacheItem ListGet(string key);

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="dataStream">The data stream on a data-source object</param>
        /// <returns></returns>
        ListCacheItem ListGet(IDataStream dataStream);

        /// <summary>
        /// Set an item in the list-cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="sourceRefresh"></param>
        /// <param name="durationInSeconds"></param>
        void ListSet(string key, IEnumerable<IEntity> list, DateTime sourceRefresh, int durationInSeconds = 0);

        /// <summary>
        /// Add an item to the list-cache
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="durationInSeconds"></param>
        void ListSet(IDataStream dataStream, int durationInSeconds = 0);

        /// <summary>
        /// Remove an item from the list-cache
        /// </summary>
        /// <param name="key"></param>
        void ListRemove(string key);

        /// <summary>
        /// Remove an item from the list cache
        /// </summary>
        /// <param name="dataStream"></param>
        void ListRemove(IDataStream dataStream);

        /// <summary>
        /// Check if it has this in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ListHas(string key);

        /// <summary>
        /// Check if it has this in the cache
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        bool ListHas(IDataStream dataStream);
        #endregion

	}
}
