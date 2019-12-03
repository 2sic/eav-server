using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Responsible for caching lists / streams. Usually used in queries or sources which have an intensive loading or querying time.
    /// </summary>
    [PublicApi]
    public class ListCache: IListCache
    {
        private static ObjectCache Cache => MemoryCache.Default;

        private static readonly ConcurrentDictionary<string, object> LoadLocks
             = new ConcurrentDictionary<string, object>();

        #region Has in Cache

        /// <inheritdoc />
        public bool Has(string key) => Cache.Contains(key);

        /// <inheritdoc />
        public bool Has(IDataStream dataStream) => Has(CacheKey(dataStream));

        #endregion


        /// <inheritdoc />
        public int DefaultDuration { get; internal set; } = 60 * 60;

        /// <summary>
        /// Returns the cache key for a data stream
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        private string CacheKey(IDataStream dataStream)
        {
            return dataStream.Source.CacheFullKey + "|" + dataStream.Name;
        }

        #region Get List

        /// <inheritdoc />
        public ListCacheItem GetOrBuild(IDataStream dataStream, Func<IEnumerable<IEntity>> builderFunc,
            int durationInSeconds = 0)
        {
            var key = CacheKey(dataStream);

            // Check if it's in the cache, and if it requires re-loading
            var itemInCache = Get(key);
            var isInCache = itemInCache != null;
            // todo: 2rm: I added a null-check because of Resharper warnings - pls check
            // todo: 2rm: I standardized how we check if the cache using the CacheChanged interface, pls check
            //var reloadCacheNeeded = dataStream.CacheRefreshOnSourceRefresh && (dataStream.Source.CacheTimestamp > itemInCache?.CacheTimestamp);
            var reloadCacheNeeded = dataStream.CacheRefreshOnSourceRefresh && (itemInCache?.CacheChanged(dataStream.Source.CacheTimestamp) ?? true);
            if (isInCache && reloadCacheNeeded)
                return itemInCache;

            // If reloading is required, set a lock first (to prevent parallel loading of the same data)
            var lockKey = LoadLocks.GetOrAdd(key, new object());
            lock (lockKey)
            {
                // todo: 2rm - recheck expiry, because maybe it was replaced in the meantime

                // now that lock is free, it could have been initialized, so re-check
                // Checking for timestamps is not needed as a previous lock would have reloaded the cache
                if (Get(key) != null)
                    return Get(key);

                var entities = builderFunc();
                Set(key, entities, durationInSeconds);

                return Get(key);
            }
        }

        /// <inheritdoc />
        public ListCacheItem Get(string key) => Cache[key] as ListCacheItem;

        /// <inheritdoc />
        public ListCacheItem Get(IDataStream dataStream) => Get(CacheKey(dataStream));

        #endregion

        #region set/add list

        /// <inheritdoc />
        public void Set(string key, IEnumerable<IEntity> list, long sourceTimestamp, int durationInSeconds = 0)
        {
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0,
                    durationInSeconds > 0 ? durationInSeconds : DefaultDuration)
            };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(/*key,*/ list, sourceTimestamp), policy);
        }

        /// <inheritdoc />
        public void Set(IDataStream dataStream, int durationInSeconds = 0)
            => Set(CacheKey(dataStream), dataStream.List,
                dataStream.Source.CacheTimestamp, durationInSeconds);


        #endregion

        #region Remove List
        /// <inheritdoc />
        public void Remove(string key) => MemoryCache.Default.Remove(key);

        /// <inheritdoc />
        public void Remove(IDataStream dataStream) => Remove(CacheKey(dataStream));
        #endregion
    }
}
