using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caching
{
    public class ListCache: IListCache
    {
        private static ObjectCache Cache => MemoryCache.Default;

        private static readonly ConcurrentDictionary<string, object> LoadLocks
             = new ConcurrentDictionary<string, object>();

        #region Has in Cache

        /// <inheritdoc />
        public bool Has(string key) => Cache.Contains(key);

        /// <inheritdoc />
        public bool Has(IDataStream dataStream) => Has(dataStream.Source.CacheFullKey + "|" + dataStream.Name);

        #endregion


        /// <inheritdoc />
        public int DefaultRetentionTime { get; set; }

        #region Get List

        /// <inheritdoc />
        public ListCacheItem GetOrBuild(IDataStream dataStream, Func<IEnumerable<IEntity>> builderFunc,
            int durationInSeconds)
        {
            var key = dataStream.Source.CacheFullKey + "|" + dataStream.Name;

            var itemInCache = Get(key);
            var isInCache = itemInCache != null;
            // todo: 2rm:
            var reloadCacheNeeded = dataStream.CacheRefreshOnSourceRefresh && (dataStream.Source.CacheTimestamp > itemInCache?.SourceTimestamp);
            if (isInCache && reloadCacheNeeded)
                return itemInCache;

            var lockKey = LoadLocks.GetOrAdd(key, new object());
            lock (lockKey)
            {
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
        public ListCacheItem Get(IDataStream dataStream) => Get(dataStream.Source.CacheFullKey + "|" + dataStream.Name);

        #endregion

        #region set/add list

        /// <inheritdoc />
        public void Set(string key, IEnumerable<IEntity> list, long sourceTimestamp, int durationInSeconds = 0)
        {
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0,
                    durationInSeconds > 0 ? durationInSeconds : DefaultRetentionTime)
            };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(/*key,*/ list, sourceTimestamp), policy);
        }

        /// <inheritdoc />
        public void Set(IDataStream dataStream, int durationInSeconds = 0)
            => Set(dataStream.Source.CacheFullKey + "|" + dataStream.Name, dataStream.List,
                dataStream.Source.CacheTimestamp, durationInSeconds);


        #endregion

        #region Remove List
        /// <inheritdoc />
        public void Remove(string key) => MemoryCache.Default.Remove(key);

        /// <inheritdoc />
        public void Remove(IDataStream dataStream) => Remove(dataStream.Source.CacheFullKey + "|" + dataStream.Name);
        #endregion
    }
}
