using System.Collections.Concurrent;
using System.Runtime.Caching;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.Caching;

/// <summary>
/// Internal cache for the data-sources
/// </summary>
[PrivateApi]
public /* should be internal as soon as insights work with that */ class DataSourceListCache
{
    #region Static Caching and Lock Variables

    internal static ObjectCache Cache => MemoryCache.Default;

    public static readonly ConcurrentDictionary<string, object> LoadLocks = new();

    #endregion

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

    public static bool HasStream(string key) => Cache.Contains(key);

    public static bool HasStream(IDataStream stream) => HasStream(CacheKey(stream));

    #endregion

    #region Get (static)


    public static ListCacheItem GetStream(string key) => Cache[key] as ListCacheItem;

    #endregion

}