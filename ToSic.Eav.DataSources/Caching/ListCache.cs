using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Responsible for caching lists / streams. Usually used in queries or sources which have an intensive loading or querying time.
    /// </summary>
    [PublicApi]
    public class ListCache: HasLog, IListCache
    {

        private static ObjectCache Cache => MemoryCache.Default;

        private static readonly ConcurrentDictionary<string, object> LoadLocks
             = new ConcurrentDictionary<string, object>();


        /// <summary>
        /// Constructor
        /// </summary>
        [PrivateApi]
        public ListCache(ILog parentLog) : base("DS.LstCch", parentLog) { }

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
        private string CacheKey(IDataStream dataStream) => dataStream.Source.CacheFullKey + "|" + dataStream.Name;

        #region Get List

        /// <summary>
        /// Returns the cache item only if it is valid:
        /// - item is in cache
        /// - cache does not expire or source did not change/expire
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        private ListCacheItem GetValidCacheItemOrNull(IDataStream dataStream)
        {
            var wrapLog = Log.Call(nameof(GetValidCacheItemOrNull));
            // Check if it's in the cache, and if it requires re-loading
            var key = CacheKey(dataStream);
            var itemInCache = Get(key);
            var valid = itemInCache != null &&
                        (!dataStream.CacheRefreshOnSourceRefresh || !itemInCache.CacheChanged(dataStream.Source.CacheTimestamp));
            wrapLog(valid.ToString());
            return valid ? itemInCache : null;
        }

        /// <inheritdoc />
        public ListCacheItem GetOrBuild(IDataStream stream, Func<IEnumerable<IEntity>> builderFunc,
            int durationInSeconds = 0)
        {
            var wrapLog = Log.Call<ListCacheItem>(nameof(GetOrBuild));
            var key = CacheKey(stream);

            var cacheItem = GetValidCacheItemOrNull(stream);
            if (cacheItem != null)
                return wrapLog("found", cacheItem);

            // If reloading is required, set a lock first (to prevent parallel loading of the same data)
            var lockKey = LoadLocks.GetOrAdd(key, new object());
            lock (lockKey)
            {
                Log.Add("came out of lock");
                // now that lock is free, it could have been initialized, so re-check
                cacheItem = GetValidCacheItemOrNull(stream);
                if (cacheItem != null)
                    return wrapLog("still valid", cacheItem);

                Log.Add($"Re-Building cache of data stream {stream.Name}");
                var entities = builderFunc();
                var useSlidingExpiration = stream.CacheRefreshOnSourceRefresh;
                Set(key, entities, stream.Source.CacheTimestamp, durationInSeconds, useSlidingExpiration);

                return wrapLog("generated", Get(key));
            }
        }

        /// <inheritdoc />
        public ListCacheItem Get(string key) => Cache[key] as ListCacheItem;

        /// <inheritdoc />
        public ListCacheItem Get(IDataStream dataStream) => Get(CacheKey(dataStream));

        #endregion

        #region set/add list

        /// <inheritdoc />
        public void Set(string key, IEnumerable<IEntity> list, long sourceTimestamp, int durationInSeconds = 0,
            bool slidingExpiration = true)
        {
            var duration = durationInSeconds > 0 ? durationInSeconds : DefaultDuration;
            var expiration = new TimeSpan(0, 0, duration);
            var policy = slidingExpiration
                ? new CacheItemPolicy { SlidingExpiration = expiration }
                : new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(duration) };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(/*key,*/ list, sourceTimestamp), policy);
        }

        /// <inheritdoc />
        public void Set(IDataStream dataStream, int durationInSeconds = 0, bool slidingExpiration = true)
            => Set(CacheKey(dataStream), dataStream.List,
                dataStream.Source.CacheTimestamp, durationInSeconds, slidingExpiration);


        #endregion

        #region Remove List
        /// <inheritdoc />
        public void Remove(string key) => MemoryCache.Default.Remove(key);

        /// <inheritdoc />
        public void Remove(IDataStream dataStream) => Remove(CacheKey(dataStream));
        #endregion
    }
}
