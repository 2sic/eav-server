namespace ToSic.Eav.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanSelfCache
{
    /// <summary>
    /// Place the stream in the cache if wanted, by default not
    /// </summary>
    bool AutoCaching { get; /*set;*/ }

    /// <summary>
    /// Default cache duration is 3600 * 24 (1 day)
    /// </summary>
    int CacheDurationInSeconds { get; set; }

    /// <summary>
    /// Kill the cache if the source data is newer than the cache-stamped data
    /// </summary>
    bool CacheRefreshOnSourceRefresh { get; set; }

}