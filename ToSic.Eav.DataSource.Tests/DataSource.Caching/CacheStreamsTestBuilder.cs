using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSource.Caching;

internal class CacheStreamsTestBuilder(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
    public (EntityIdFilter, CacheAllStreams) CreateFilterAndCache(int testItemsInRootSource, string entityIdsValue)
    {
        var filtered = CreateFilterForTesting(testItemsInRootSource, entityIdsValue);
        var cacheDs = CreateCacheDs(filtered);
        return (filtered, cacheDs);
    }

    public CacheAllStreams CreateCacheDs(IDataSource filtered, object? options = null)
    {
        var cacheAll = dsSvc.CreateDataSourceNew<CacheAllStreams>(options);
        cacheAll.AttachTac(filtered);
        return cacheAll;
    }

    public EntityIdFilter CreateFilterForTesting(int itemsToGenerateInRoot, string entityIdsValue, bool useCacheForSpeed = true)
    {
        var ds = personTable.Generate(itemsToGenerateInRoot, 1001, useCacheForSpeed);
        var filtered = dsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachTac(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }

}