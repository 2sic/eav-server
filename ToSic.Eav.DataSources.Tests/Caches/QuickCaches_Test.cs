using System.Collections.Immutable;
using System.Threading;
using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSourceTests.Caches;

[TestClass]
public class QuickCachesTest: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();
    public IListCacheSvc GetTestListCache() => GetService<IListCacheSvc>(); // new ListCache(new Log("test"));

    [TestMethod]
    public void QuickCache_AddListAndCheckIfIn()
    {
        const string ItemToFilter = "1023";
        var ds = CreateFilterForTesting(100, ItemToFilter);

        //var cache = ds.Root;
        var listCache = GetTestListCache();
        IsFalse(listCache.HasStreamTac(ds.CacheFullKey), "Should not have it in cache yet");

        // manually add to cache
        listCache.Set(ds[DataSourceConstants.StreamDefaultName]);
        IsTrue(listCache.HasStreamTac(ds.CacheFullKey + "&Stream=Default"), "Should have it in cache now");
        IsTrue(listCache.HasStreamTac(ds[DataSourceConstants.StreamDefaultName]), "Should also have the DS default");
            
        IsTrue(listCache.HasStreamTac(ds[DataSourceConstants.StreamDefaultName]), "should have it by stream as well");
            

        // Try to auto-retrieve 
        var cached = listCache.GetTac(ds.CacheFullKey + "&Stream=Default").List;

        AreEqual(1, cached.Count);

        cached = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]).List;
        AreEqual(1, cached.Count);

        var lci = listCache.GetTac(ds.CacheFullKey);
        AreEqual(null, lci, "Cached should be null because the name isn't correct");

        lci = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]);
        AreNotEqual(null, lci, "Cached should be found because using stream instead of name");

        cached = listCache.GetTac(ds[DataSourceConstants.StreamDefaultName]).List;
        AreEqual(1, cached.Count());

    }

    [TestMethod]
    public void QuickCache_AddAndWaitForReTimeout()
    {
        const string ItemToFilter = "1027";
        var ds = CreateFilterForTesting(100, ItemToFilter);

        var listCache = GetTestListCache();
        //listCache.DefaultDuration = 1;
        IsFalse(listCache.HasStreamTac(ds.CacheFullKey), "Should not have it in cache yet");

        listCache.Set(ds.CacheFullKey, ds.ListTac().ToImmutableList(), ds.CacheTimestamp, false, durationInSeconds: 1);
        IsTrue(listCache.HasStreamTac(ds.CacheFullKey), "Should have it in cache now");

        Thread.Sleep(400);
        IsTrue(listCache.HasStreamTac(ds.CacheFullKey), "Should STILL be in cache");

        Thread.Sleep(601);
        IsFalse(listCache.HasStreamTac(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
    }




    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachTac(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}