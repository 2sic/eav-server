using System.Collections.Generic;
using System.Runtime.Caching;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        /// <summary>
        /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
        /// </summary>
        private static MemoryCache AppCache => MemoryCache.Default;

        private static string AppCacheKey(int appId) => $"DataSourceCatalog:AppDataSource:{appId}";

        public List<DataSourceInfo> Get(int appId)
        {
            if (AppCache[AppCacheKey(appId)] is List<DataSourceInfo> dataFromCache) return dataFromCache;

            var (data, policy) = _appDataSourcesLoader.Value.CreateAndReturnAppCache(appId);
            AppCache.Set(new CacheItem(AppCacheKey(appId), data), policy);
            return data;
        }
    }
}
