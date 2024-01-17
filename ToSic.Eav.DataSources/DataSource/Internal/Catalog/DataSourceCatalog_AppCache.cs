using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.Catalog;

partial class DataSourceCatalog
{
    /// <summary>
    /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
    /// </summary>
    private static MemoryCache AppDataSourceCache => MemoryCache.Default;

    private static string AppCacheKey(int appId) => $"DataSourceCatalog:AppDataSource:{appId}";

    public List<DataSourceInfo> Get(int appId)
    {
        if (AppDataSourceCache[AppCacheKey(appId)] is List<DataSourceInfo> dataFromCache) return dataFromCache;

        var (data, policy) = _appDataSourcesLoader.Value.CompileDynamicDataSources(appId);
        AppDataSourceCache.Set(new(AppCacheKey(appId), data), policy);
        return data;
    }
}