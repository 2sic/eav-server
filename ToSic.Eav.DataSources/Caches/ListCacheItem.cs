using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Caches
{
    /// <summary>
    /// This class is used to add lists to the cache. It contains the list + caching-data to ensure we can check the expiry if needed
    /// </summary>
    public class ListCacheItem
    {
        /// <summary>
        /// The list in the cache
        /// </summary>
        public IEnumerable<IEntity> LightList { get; set; }
        public string CacheKey { get; set; }


        /// <summary>
        /// The age of the data - to see if it needs refreshing if the new source has a newer date
        /// </summary>
        public /*DateTime*/long SourceRefresh { get; set; }

        /// <summary>
        /// Initialize the object - ready to cache
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sourceRefresh"></param>
        public ListCacheItem(string cacheKey, IEnumerable<IEntity> list, /*DateTime*/ long sourceRefresh)
        {
            CacheKey = cacheKey;
            LightList = list;
            SourceRefresh = sourceRefresh;
        }
    }
}
