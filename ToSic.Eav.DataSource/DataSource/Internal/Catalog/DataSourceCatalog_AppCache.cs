using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.Catalog;

partial class DataSourceCatalog
{

    private static string AppCacheKey(int appId) => $"DataSourceCatalog:AppDataSource:{appId}";

    public List<DataSourceInfo> Get(int appId)
    {
        if (memoryCacheService.TryGet<List<DataSourceInfo>>(AppCacheKey(appId), out var dataFromCache) && dataFromCache != null)
            return dataFromCache;

        var appLocalDataSources = appDataSourcesLoader.Value.CompileDynamicDataSources(appId);

        memoryCacheService.Set(
            AppCacheKey(appId),
            appLocalDataSources.Data,
            policyMaker => policyMaker
                .SetSlidingExpiration(appLocalDataSources.SlidingExpiration)
                .WatchFolders(appLocalDataSources.FolderPaths.ToDictionary(pair => pair, _ => true))
                .WatchCacheKeys(appLocalDataSources.CacheKeys)
        );

        return appLocalDataSources.Data;
    }
}