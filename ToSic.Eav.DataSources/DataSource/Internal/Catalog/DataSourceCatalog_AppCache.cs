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
        if (_memoryCacheService.Get(AppCacheKey(appId)) is List<DataSourceInfo> dataFromCache) return dataFromCache;

        var (data, slidingExpiration, folderPaths, cacheKeys) = _appDataSourcesLoader.Value.CompileDynamicDataSources(appId);

        _memoryCacheService.Set(AppCacheKey(appId), data, 
            slidingExpiration: slidingExpiration,
            folderPaths: folderPaths.ToDictionary(p => p, p => true),
            cacheKeys: cacheKeys);

        return data;
    }
}