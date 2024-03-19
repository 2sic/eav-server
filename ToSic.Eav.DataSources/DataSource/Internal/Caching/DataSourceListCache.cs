using System.Collections.Concurrent;
using System.Runtime.Caching;
using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// Internal cache for the data-sources
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public /* should be internal as soon as insights work with that */
    class DataSourceListCache(MemoryCacheService memoryCacheService)
{
    #region Static Lock Variables

    public static readonly ConcurrentDictionary<string, object> LoadLocks = new();

    #endregion

    /// <summary>
    /// The time a list stays in the cache by default - default is 3600 = 1 hour.
    /// Is used in all Set commands where the default duration is needed.
    /// </summary>
    internal const int DefaultDuration = 60 * 60;


    #region Cache Checks

    /// <summary>
    /// Returns the cache key for a data stream
    /// </summary>
    /// <param name="dataStream"></param>
    /// <returns></returns>
    internal static string CacheKey(IDataStream dataStream) => dataStream.Caching.CacheFullKey;

    public bool HasStream(string key) => memoryCacheService.Contains(key);

    public bool HasStream(IDataStream stream) => HasStream(CacheKey(stream));

    #endregion

    #region Get

    public ListCacheItem GetStream(string key) => memoryCacheService.Get<ListCacheItem>(key);

    #endregion

    #region Add
    public void Add(string key, object value, CacheItemPolicy policy) => memoryCacheService.Set(key, value, policy);
    #endregion
}