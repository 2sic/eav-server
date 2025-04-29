using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources.Caching;

namespace ToSic.Eav.DataSource.Caching;

internal class CacheStreamsTestBuilder(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
    public (EntityIdFilter, CacheAllStreams) CreateFilterAndCache(int testItemsInRootSource, string entityIdsValue)
    {
        var filtered = CreateFilterForTesting(testItemsInRootSource, entityIdsValue);
        var cacheDs = CreateCacheDs(filtered);
        return (filtered, cacheDs);
    }

    public (IDataSource Source, CacheAllStreams Cache) CreateErrorAndCache(int errDuration, int delayError)
    {
        var source = delayError == 0
            ? dsSvc.CreateDataSource<Error>()
            : dsSvc.CreateDataSourceNew<Error>(options: new { DelaySeconds = delayError});
        var cacheDs = CreateCacheDs(source, options: new { CacheErrorDurationInSeconds = errDuration });
        return (source, cacheDs);
    }

    public CacheAllStreams CreateCacheDs(IDataSource source, object? options = null)
    {
        var cacheAll = dsSvc.CreateDataSourceNew<CacheAllStreams>(options);
        cacheAll.AttachTac(source);
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