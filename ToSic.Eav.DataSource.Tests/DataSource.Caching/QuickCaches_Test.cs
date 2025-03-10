using System.Collections.Immutable;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSource.Caching;

[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class QuickCachesTest(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
    [Fact]
    public void QuickCache_AddListAndCheckIfIn()
    {
        const string itemToFilter = "1023";
        var ds = CreateFilterForTesting(100, itemToFilter);

        False(listCache.HasStreamTac(ds.CacheFullKey), "Should not have it in cache yet");

        // manually add to cache
        listCache.Set(ds[DataSourceConstants.StreamDefaultName]);
        True(listCache.HasStreamTac(ds.CacheFullKey + "&Stream=Default"), "Should have it in cache now");
        True(listCache.HasStreamTac(ds[DataSourceConstants.StreamDefaultName]), "Should also have the DS default");
            
        True(listCache.HasStreamTac(ds[DataSourceConstants.StreamDefaultName]), "should have it by stream as well");
            

        // Try to auto-retrieve 
        var cached = listCache.GetTac(ds.CacheFullKey + "&Stream=Default").List;

        Single(cached);

        cached = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]).List;
        Single(cached);

        var lci = listCache.GetTac(ds.CacheFullKey);
        Null(lci);//, "Cached should be null because the name isn't correct");

        lci = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]);
        NotNull(lci);//, "Cached should be found because using stream instead of name");

        cached = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]).List;
        Single(cached);

    }

    [Fact]
    public void QuickCache_AddAndWaitForReTimeout()
    {
        const string itemToFilter = "1027";
        var ds = CreateFilterForTesting(100, itemToFilter);

        False(listCache.HasStreamTac(ds.CacheFullKey), "Should not have it in cache yet");

        listCache.Set(ds.CacheFullKey, ds.ListTac().ToImmutableList(), ds.CacheTimestamp, false, durationInSeconds: 1);
        True(listCache.HasStreamTac(ds.CacheFullKey), "Should have it in cache now");

        Thread.Sleep(400);
        True(listCache.HasStreamTac(ds.CacheFullKey), "Should STILL be in cache");

        Thread.Sleep(601);
        False(listCache.HasStreamTac(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
    }




    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
    {
        var ds = personTable.Generate(testItemsInRootSource, 1001);
        var filtered = dsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachTac(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}