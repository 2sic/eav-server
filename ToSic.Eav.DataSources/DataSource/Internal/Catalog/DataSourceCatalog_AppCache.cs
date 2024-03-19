using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.Catalog;

partial class DataSourceCatalog
{
    /// <summary>
    /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
    /// </summary>



    public List<DataSourceInfo> Get(int appId)
    {
        var appCacheKey = MemoryCacheService.DataSourceCatalogAppCacheKey(appId);

        if (memoryCacheService.TryGetValue(appCacheKey, out List<DataSourceInfo> dataFromCache))
            return dataFromCache;

        var (data, policy) = appDataSourcesLoader.Value.CompileDynamicDataSources(appId);
        memoryCacheService.Set(appCacheKey, data, policy);
        return data;

        //CacheItemPolicy policy = null;
        //return _memoryCacheService.GetOrBuild(appCacheKey, () =>
        //{
        //    (var data, policy) = _appDataSourcesLoader.Value.CompileDynamicDataSources(appId);
        //    return data;
        //}, policy);
    }
}