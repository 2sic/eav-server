using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources.Caches
{
    /// <summary>
    /// Provide ability to cache lists of entities
    /// </summary>
    public interface IListsCache
    {
        /// <summary>
        /// The time a list stays in the cache by default - usually 3600 = 1 hour
        /// </summary>
        int ListDefaultRetentionTimeInSeconds { get; set; }

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ListCacheItem Get(string key);

        /// <summary>
        /// Get a list from the cache
        /// </summary>
        /// <param name="dataStream">The data stream on a data-source object</param>
        /// <returns></returns>
        ListCacheItem Get(IDataStream dataStream);

        /// <summary>
        /// Set an item in the list-cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="sourceRefresh"></param>
        /// <param name="durationInSeconds"></param>
        void Set(string key, IEnumerable<IEntity> list, long sourceRefresh, int durationInSeconds = 0);

        /// <summary>
        /// Add an item to the list-cache
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="durationInSeconds"></param>
        void Set(IDataStream dataStream, int durationInSeconds = 0);

        /// <summary>
        /// Remove an item from the list-cache
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

        /// <summary>
        /// Remove an item from the list cache
        /// </summary>
        /// <param name="dataStream"></param>
        void Remove(IDataStream dataStream);

        /// <summary>
        /// Check if it has this in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Has(string key);

        /// <summary>
        /// Check if it has this in the cache
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        bool Has(IDataStream dataStream);
    }
}
