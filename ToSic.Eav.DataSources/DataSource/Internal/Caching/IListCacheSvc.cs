namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// Marks objects that can cache lists based on certain rules - including retention time and if up-stream changes should refresh the cache. 
/// </summary>
[PrivateApi("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IListCacheSvc
{
    /// <summary>
    /// Get a list from the cache
    /// </summary>
    /// <param name="key">the identifier in the cache</param>
    /// <returns>the cached list</returns>
    ListCacheItem Get(string key);

    /// <summary>
    /// Get a list from the cache using a configured dataStream. The stream won't be queried, it serves as an identifier for the cache item. 
    /// </summary>
    /// <param name="dataStream">the data stream, which can provide it's cache-key</param>
    /// <returns>the cached list</returns>
    ListCacheItem Get(IDataStream dataStream);

    /// <summary>
    /// Get cached item if available and valid, or rebuild cache using a mutual lock
    /// </summary>
    /// <param name="stream">The data stream on a data-source object</param>
    /// <param name="builderFunc">a function which is only called if building is required</param>
    /// <param name="durationInSeconds">The cache validity duration in seconds. If 0 or omitted, default value will be used. </param>
    /// <returns>The ListCacheItem - either from cache, or just created</returns>
    ListCacheItem GetOrBuild(IDataStream stream, Func<IImmutableList<IEntity>> builderFunc, int durationInSeconds = 0);

    /// <summary>
    /// Add an item to the list-cache
    /// </summary>
    /// <param name="key">cache key</param>
    /// <param name="list">items to put into the cache for this cache key</param>
    /// <param name="sourceTimestamp"></param>
    /// <param name="durationInSeconds">The cache validity duration in seconds. If 0 or omitted, default value will be used. </param>
    /// <param name="slidingExpiration"></param>
    void Set(string key, IImmutableList<IEntity> list, long sourceTimestamp, bool refreshOnSourceRefresh, int durationInSeconds = 0, bool slidingExpiration = true);

    /// <summary>
    /// Add an item to the list-cache
    /// </summary>
    /// <param name="dataStream">the data stream, which can provide it's cache-key</param>
    /// <param name="durationInSeconds">The cache validity duration in seconds. If 0 or omitted, default value will be used. </param>
    /// <param name="slidingExpiration"></param>
    void Set(IDataStream dataStream, int durationInSeconds = 0, bool slidingExpiration = true);

}