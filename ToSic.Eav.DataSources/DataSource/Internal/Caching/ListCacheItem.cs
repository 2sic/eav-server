﻿using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// Cache Item in the List-Cache. 
/// </summary>
[PrivateApi("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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

    // Ported 2024-10-22 - remove old code ca. 2024-12 #MemoryCacheApiCleanUp
    // don't think it's in use any where
    //public CacheItemPolicy Policy { get; }

    /// <summary>
    /// Initialize the object - ready to cache
    /// </summary>
    /// <param name="list">The list of items to put into the cache.</param>
    /// <param name="cacheTimestamp">The timestamp of the source at the moment of cache-buildup, to later detect changes in the source.</param>
    /// <param name="refreshOnSourceRefresh"></param>
    ///// <param name="cacheKey">The cache key as it is stored in the cache</param>
    public ListCacheItem(IImmutableList<IEntity> list, long cacheTimestamp, bool refreshOnSourceRefresh)
    // Ported 2024-10-22 - remove old code ca. 2024-12 #MemoryCacheApiCleanUp
    //public ListCacheItem(IImmutableList<IEntity> list, long cacheTimestamp, bool refreshOnSourceRefresh, CacheItemPolicy policy)
    {
        List = list;
        CacheTimestamp = cacheTimestamp;
        RefreshOnSourceRefresh = refreshOnSourceRefresh;
        // Ported 2024-10-22 - remove old code ca. 2024-12 #MemoryCacheApiCleanUp
        //Policy = policy;
    }
}