using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.Catalog;

partial class DataSourceCatalog
{
    /// <summary>
    /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
    /// </summary>

    private static string AppCacheKey(int appId) => $"DataSourceCatalog:AppDataSource:{appId}";

    public List<DataSourceInfo> Get(int appId)
    {
        if (memoryCacheService.TryGet<List<DataSourceInfo>>(AppCacheKey(appId), out var dataFromCache))
            return dataFromCache;

        var (data, slidingExpiration, folderPaths, cacheKeys)
            = appDataSourcesLoader.Value.CompileDynamicDataSources(appId);

        memoryCacheService.Set(AppCacheKey(appId), data, 
            slidingExpiration: slidingExpiration,
            folderPaths: folderPaths?.ToDictionary(p => p, p => true),
            cacheKeys: cacheKeys);

        return data;
    }
}