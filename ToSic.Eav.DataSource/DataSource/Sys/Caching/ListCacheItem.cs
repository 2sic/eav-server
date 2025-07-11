﻿using ToSic.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Caching;

/// <summary>
/// Cache Item in the List-Cache. 
/// </summary>
[PrivateApi("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ListCacheItem: ICacheExpiring
{
    /// <summary>
    /// The items which we're caching.
    /// </summary>
    public IImmutableList<IEntity> List { get; set; }

    /// <summary>
    /// The age of the data - to see if it needs refreshing if the new source has a newer date
    /// </summary>
    public long CacheTimestamp { get; set; }

    /// <inheritdoc/>
    public bool CacheChanged(long dependentTimeStamp) => dependentTimeStamp != CacheTimestamp;

    public bool RefreshOnSourceRefresh { get; }

    /// <summary>
    /// Initialize the object - ready to cache
    /// </summary>
    /// <param name="list">The list of items to put into the cache.</param>
    /// <param name="cacheTimestamp">The timestamp of the source at the moment of cache-buildup, to later detect changes in the source.</param>
    /// <param name="refreshOnSourceRefresh"></param>
    ///// <param name="cacheKey">The cache key as it is stored in the cache</param>
    public ListCacheItem(IImmutableList<IEntity> list, long cacheTimestamp, bool refreshOnSourceRefresh)
    {
        List = list;
        CacheTimestamp = cacheTimestamp;
        RefreshOnSourceRefresh = refreshOnSourceRefresh;
    }
}