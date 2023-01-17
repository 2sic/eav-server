using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.Caching;
using ToSic.Eav.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Responsible for caching lists / streams. Usually used in queries or sources which have an intensive loading or querying time.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class ListCache: HelperBase, IListCache
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
        public ListCache(ILog parentLog = null) : base(parentLog, "DS.LstCch") { }

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
        private ListCacheItem GetValidCacheItemOrNull(IDataStream dataStream) => Log.Func(l =>
        {
            // Check if it's in the cache, and if it requires re-loading
            var key = CacheKey(dataStream);
            var itemInCache = Get(key);
            var found = itemInCache != null;
            var valid = found && (!dataStream.CacheRefreshOnSourceRefresh || !itemInCache.CacheChanged(dataStream.Caching.CacheTimestamp));
            l.A($"ListCache found:{found}; valid:{valid}; timestamp:{dataStream.Caching.CacheTimestamp} = {dataStream.Caching.CacheTimestamp.ToReadable()}");
            l.A($"ListCache key:'{key}'");
            return (valid ? itemInCache : null, valid.ToString());
        });

        /// <inheritdoc />
        public ListCacheItem GetOrBuild(IDataStream stream, Func<IImmutableList<IEntity>> builderFunc,
            int durationInSeconds = 0) => Log.Func(l =>
        {
            var key = CacheKey(stream);

            var cacheItem = GetValidCacheItemOrNull(stream);
            if (cacheItem != null)
                return (cacheItem, "found, use cache");

            // If reloading is required, set a lock first
            // This is super important to prevent parallel loading of the same data
            // Otherwise slow loading data - like SharePoint lists from a remote server
            // would trigger multiple load attempts on page reloads and overload the system
            // trying to reload while still building the initial cache
            var lockKey = LoadLocks.GetOrAdd(key, new object());
            lock (lockKey)
            {
                l.A("came out of lock");
                // now that lock is free, it could have been initialized, so re-check
                cacheItem = GetValidCacheItemOrNull(stream);
                if (cacheItem != null)
                    return (cacheItem, "still valid, use cache");

                l.A($"Re-Building cache of data stream {stream.Name}");
                var entities = builderFunc();
                var useSlidingExpiration = stream.CacheRefreshOnSourceRefresh;
                Set(key, entities, stream.Caching.CacheTimestamp, durationInSeconds, useSlidingExpiration);

                return (Get(key), "generated and placed in cache");
            }
        });

        /// <inheritdoc />
        public ListCacheItem Get(string key) => Cache[key] as ListCacheItem;

        /// <inheritdoc />
        public ListCacheItem Get(IDataStream dataStream) => Get(CacheKey(dataStream));

        #endregion

        #region set/add list

        /// <inheritdoc />
        public void Set(string key, IImmutableList<IEntity> list, long sourceTimestamp, int durationInSeconds = 0,
            bool slidingExpiration = true
        ) => Log.Do($"key: {key}; sourceTime: {sourceTimestamp}; duration:{durationInSeconds}; sliding: {slidingExpiration}", () =>
        {
            var duration = durationInSeconds > 0 ? durationInSeconds : DefaultDuration;
            var expiration = new TimeSpan(0, 0, duration);
            var policy = slidingExpiration
                ? new CacheItemPolicy { SlidingExpiration = expiration }
                : new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(duration) };

            var cache = MemoryCache.Default;
            cache.Set(key, new ListCacheItem(list, sourceTimestamp), policy);
        });
        

        /// <inheritdoc />
        public void Set(IDataStream dataStream, int durationInSeconds = 0, bool slidingExpiration = true)
            => Set(CacheKey(dataStream), dataStream.List.ToImmutableArray(),
                dataStream.Caching.CacheTimestamp, durationInSeconds, slidingExpiration);

        #endregion

        #region Remove List

        /// <inheritdoc />
        public void Remove(string key) => Log.Do(() => MemoryCache.Default.Remove(key));

        /// <inheritdoc />
        public void Remove(IDataStream dataStream) => Remove(CacheKey(dataStream));
        #endregion
    }
}
