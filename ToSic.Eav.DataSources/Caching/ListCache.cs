using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Responsible for caching lists / streams. Usually used in queries or sources which have an intensive loading or querying time.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class ListCache: HasLog, IListCache
    {
        #region Static Caching and Lock Variables
        private static ObjectCache Cache => MemoryCache.Default;

        private static readonly ConcurrentDictionary<string, object> LoadLocks
             = new ConcurrentDictionary<string, object>();
        #endregion

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
        private string CacheKey(IDataStream dataStream) => dataStream.Caching.CacheFullKey;

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
            var wrapLog = Log.Call();
            // Check if it's in the cache, and if it requires re-loading
            var key = CacheKey(dataStream);
            var itemInCache = Get(key);
            var found = itemInCache != null;
            var valid = found && (!dataStream.CacheRefreshOnSourceRefresh || !itemInCache.CacheChanged(dataStream.Caching.CacheTimestamp));
            Log.Add($"ListCache found:{found}; valid:{valid}; timestamp:{dataStream.Caching.CacheTimestamp}");
            Log.Add($"ListCache key:'{key}'");
            wrapLog(valid.ToString());
            return valid ? itemInCache : null;
        }

        /// <inheritdoc />
        public ListCacheItem GetOrBuild(IDataStream stream, Func<IImmutableList<IEntity>> builderFunc,
            int durationInSeconds = 0)
        {
            var wrapLog = Log.Call<ListCacheItem>();
            var key = CacheKey(stream);

            var cacheItem = GetValidCacheItemOrNull(stream);
            if (cacheItem != null)
                return wrapLog("found, use cache", cacheItem);

            // If reloading is required, set a lock first
            // This is super important to prevent parallel loading of the same data
            // Otherwise slow loading data - like SharePoint lists from a remote server
            // would trigger multiple load attempts on page reloads and overload the system
            // trying to reload while still building the initial cache
            var lockKey = LoadLocks.GetOrAdd(key, new object());
            lock (lockKey)
            {
                Log.Add("came out of lock");
                // now that lock is free, it could have been initialized, so re-check
                cacheItem = GetValidCacheItemOrNull(stream);
                if (cacheItem != null)
                    return wrapLog("still valid, use cache", cacheItem);

                Log.Add($"Re-Building cache of data stream {stream.Name}");
                var entities = builderFunc();
                var useSlidingExpiration = stream.CacheRefreshOnSourceRefresh;
                Set(key, entities, stream.Caching.CacheTimestamp, durationInSeconds, useSlidingExpiration);

                return wrapLog("generated and placed in cache", Get(key));
            }
        }

        /// <inheritdoc />
        public ListCacheItem Get(string key) => Cache[key] as ListCacheItem;

        /// <inheritdoc />
        public ListCacheItem Get(IDataStream dataStream) => Get(CacheKey(dataStream));

        #endregion

        #region set/add list

        /// <inheritdoc />
        public void Set(string key, IImmutableList<IEntity> list, long sourceTimestamp, int durationInSeconds = 0,
            bool slidingExpiration = true)
        {
            var callLog = Log.Call($"key: {key}; sourceTime: {sourceTimestamp}; duration:{durationInSeconds}; sliding: {slidingExpiration}");
            var duration = durationInSeconds > 0 ? durationInSeconds : DefaultDuration;
            var expiration = new TimeSpan(0, 0, duration);
            var policy = slidingExpiration
                ? new CacheItemPolicy { SlidingExpiration = expiration }
                : new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(duration) };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(list, sourceTimestamp), policy);
            callLog("ok");
        }


        /// <inheritdoc />
        public void Set(IDataStream dataStream, int durationInSeconds = 0, bool slidingExpiration = true)
            // todo: drop toimmutablearray again
            => Set(CacheKey(dataStream), dataStream.Immutable.ToImmutableArray(),
                dataStream.Caching.CacheTimestamp, durationInSeconds, slidingExpiration);

        //public void Set(string key, IEnumerable<IEntity> list, long sourceTimestamp, int durationInSeconds = 0, bool slidingExpiration = true) 
        //    => Set(key, list.ToImmutableArray(), sourceTimestamp, durationInSeconds, slidingExpiration);

        #endregion

        #region Remove List
        /// <inheritdoc />
        public void Remove(string key)
        {
            var callLog = Log.Call(key);
            MemoryCache.Default.Remove(key);
            callLog("ok");
        }

        /// <inheritdoc />
        public void Remove(IDataStream dataStream) => Remove(CacheKey(dataStream));
        #endregion
    }
}
