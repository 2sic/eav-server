using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caches
{
    public class ListsCache: IListsCache
    {
        // todo: check what happens with this in a DNN environment; I guess it works, but there are risks...
        private ObjectCache ListCache => MemoryCache.Default;

        #region Has List
        public bool Has(string key) => ListCache.Contains(key);

        public bool Has(IDataStream dataStream) => Has(dataStream.Source.CacheFullKey + "|" + dataStream.Name);

        #endregion


        public int ListDefaultRetentionTimeInSeconds { get; set; }

        #region Get List
        /// <summary>
        /// Get a DataStream in the cache - will be null if not found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ListCacheItem Get(string key)
            => ListCache[key] as ListCacheItem;

        public ListCacheItem Get(IDataStream dataStream)
            => Get(dataStream.Source.CacheFullKey + "|" + dataStream.Name);

        #endregion

        #region set/add list

        /// <inheritdoc />
        /// <summary>
        /// Insert a data-stream to the cache - if it can be found
        /// </summary>
        public void Set(string key, IEnumerable<IEntity> list, long sourceRefresh, int durationInSeconds = 0)
        {
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0,
                    durationInSeconds > 0 ? durationInSeconds : ListDefaultRetentionTimeInSeconds)
            };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(key, list, sourceRefresh), policy);
        }

        public void Set(IDataStream dataStream, int durationInSeconds = 0)
            => Set(dataStream.Source.CacheFullKey + "|" + dataStream.Name, dataStream.List,
                dataStream.Source.CacheTimestamp, durationInSeconds);


        #endregion

        #region Remove List
        public void Remove(string key) => MemoryCache.Default.Remove(key);

        public void Remove(IDataStream dataStream)
            => Remove(dataStream.Source.CacheFullKey + "|" + dataStream.Name);
        #endregion
    }
}
