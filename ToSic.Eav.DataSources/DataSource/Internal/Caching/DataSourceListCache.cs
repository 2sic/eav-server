using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSource.Internal.Caching;

// TODO: @STV - I think this should be merged into the ListCacheSvc as it seems to be a leftover and only does very little

/// <summary>
/// Internal cache for the data-sources
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public /* should be internal as soon as insights work with that */ class DataSourceListCache(MemoryCacheService memoryCacheService)
{

    #region Static Caching and Lock Variables

    public static readonly NamedLocks LoadLocks = new();

    #endregion


    #region Static Cache Checks

    /// <summary>
    /// Returns the cache key for a data stream
    /// </summary>
    /// <param name="dataStream"></param>
    /// <returns></returns>
    internal static string CacheKey(IDataStream dataStream) => dataStream.Caching.CacheFullKey;

    public bool HasStream(string key) => memoryCacheService.Contains(key);

    #endregion

    #region Get (static)


    public ListCacheItem GetStream(string key) => memoryCacheService.Get(key) as ListCacheItem;

    #endregion

}