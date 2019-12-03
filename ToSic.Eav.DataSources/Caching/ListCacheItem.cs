﻿using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using NotImplementedException = System.NotImplementedException;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Cache Item in the List-Cache. 
    /// </summary>
    [PublicApi]
    public class ListCacheItem: ICacheExpiring
    {
        /// <summary>
        /// The items which we're caching.
        /// </summary>
        public IEnumerable<IEntity> List { get; set; }

        ///// <summary>
        ///// The key as the item is stored in the cache. More for internal use.
        ///// </summary>
        //[PrivateApi]
        //public string CacheKey { get; set; }


        /// <summary>
        /// The age of the data - to see if it needs refreshing if the new source has a newer date
        /// </summary>
        public long CacheTimestamp { get; set; }

        /// <inheritdoc/>
        public bool CacheChanged(long newCacheTimeStamp) => newCacheTimeStamp != CacheTimestamp;

        /// <summary>
        /// Initialize the object - ready to cache
        /// </summary>
        /// <param name="list">The list of items to put into the cache.</param>
        /// <param name="cacheTimestamp">The timestamp of the source at the moment of cache-buildup, to later detect changes in the source.</param>
        ///// <param name="cacheKey">The cache key as it is stored in the cache</param>
        public ListCacheItem(/*string cacheKey,*/ IEnumerable<IEntity> list, long cacheTimestamp)
        {
            //CacheKey = cacheKey;
            List = list;
            CacheTimestamp = cacheTimestamp;
        }
    }
}
