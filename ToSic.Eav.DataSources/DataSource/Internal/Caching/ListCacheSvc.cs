using ToSic.Eav.Caching;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// Responsible for caching lists / streams.
/// Usually used in queries or sources which have an intensive loading or querying time.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
[PrivateApi("this is just fyi")]
[method: PrivateApi]
internal class ListCacheSvc(MemoryCacheService memoryCacheService) : ServiceBase("DS.LstCch", connect: [memoryCacheService]), IListCacheSvc
{
    /// <summary>
    /// The time a list stays in the cache by default - default is 3600 = 1 hour.
    /// Is used in all Set commands where the default duration is needed.
    /// </summary>
    internal const int DefaultDuration = 60 * 60;

    #region Static Cache Checks

    /// <summary>
    /// Returns the cache key for a data stream
    /// </summary>
    /// <param name="dataStream"></param>
    /// <returns></returns>
    internal static string CacheKey(IDataStream dataStream) => dataStream.Caching.CacheFullKey;

    private static readonly NamedLocks StaticLoadLocks = new();

    public NamedLocks LoadLocks => StaticLoadLocks;

    #endregion

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
        var key = CacheKey(dataStream);
        var l = Log.Fn<ListCacheItem>($"key: {key}");
        // Check if it's in the cache, and if it requires re-loading
        var itemInCache = Get(key);
        if (itemInCache == null) 
            return l.Return(null, "not in cache");
        var valid = !dataStream.CacheRefreshOnSourceRefresh || !itemInCache.CacheChanged(dataStream.Caching.CacheTimestamp);
        l.A($"ListCache found:{true}; valid:{valid}; timestamp:{dataStream.Caching.CacheTimestamp} = {dataStream.Caching.CacheTimestamp.ToReadable()}");
        return l.Return(valid ? itemInCache : null, valid.ToString());
    }

    /// <inheritdoc />
    public ListCacheItem GetOrBuild(IDataStream stream, Func<IImmutableList<IEntity>> builderFunc, int durationInSeconds = 0)
    {
        var l = Log.Fn<ListCacheItem>();
        var key = CacheKey(stream);

        var cacheItem = GetValidCacheItemOrNull(stream);
        if (cacheItem != null)
            return l.Return(cacheItem, "found, use cache");

        // If reloading is required, set a lock first
        // This is super important to prevent parallel loading of the same data
        // Otherwise slow loading data - like SharePoint lists from a remote server
        // would trigger multiple load attempts on page reloads and overload the system
        // trying to reload while still building the initial cache
        var lockKey = LoadLocks.Get(key);
        lock (lockKey)
        {
            l.A("came out of lock");
            // now that lock is free, it could have been initialized, so re-check
            cacheItem = GetValidCacheItemOrNull(stream);
            if (cacheItem != null)
                return l.Return(cacheItem, "still valid, use cache");

            l.A($"Re-Building cache of data stream {stream.Name}");
            var entities = builderFunc();
            var useSlidingExpiration = stream.CacheRefreshOnSourceRefresh;
            Set(key, entities, stream.Caching.CacheTimestamp, stream.CacheRefreshOnSourceRefresh, durationInSeconds, useSlidingExpiration);

            return l.Return(Get(key), "generated and placed in cache");
        }
    }

    /// <inheritdoc />
    public ListCacheItem Get(string key) => memoryCacheService.Get<ListCacheItem>(key);

    /// <inheritdoc />
    public ListCacheItem Get(IDataStream dataStream) => Get(CacheKey(dataStream));

    public bool HasStream(string key) => memoryCacheService.Contains(key);

    public bool HasStream(IDataStream stream) => HasStream(CacheKey(stream));

    #endregion

    #region set/add list

    /// <inheritdoc />
    public void Set(string key, IImmutableList<IEntity> list, long sourceTimestamp, bool refreshOnSourceRefresh, int durationInSeconds = 0, bool slidingExpiration = true)
    {
        var l = Log.Fn($"key: {key}; sourceTime: {sourceTimestamp}; duration:{durationInSeconds}; sliding: {slidingExpiration}");
        var duration = durationInSeconds > 0 ? durationInSeconds : DefaultDuration;
        memoryCacheService.SetNew(key, value: new ListCacheItem(list, sourceTimestamp, refreshOnSourceRefresh), p => slidingExpiration
            ? p.SetSlidingExpiration(duration)
            : p.SetAbsoluteExpiration(DateTime.Now.AddSeconds(duration)));
        l.Done();
    }
        

    /// <inheritdoc />
    public void Set(IDataStream dataStream, int durationInSeconds = 0, bool slidingExpiration = true)
        => Set(CacheKey(dataStream), dataStream.List.ToImmutableList(),
            dataStream.Caching.CacheTimestamp, dataStream.CacheRefreshOnSourceRefresh, durationInSeconds, slidingExpiration);

    #endregion
}