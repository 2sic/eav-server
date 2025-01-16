using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Caches;

[TestClass]
public class QuickCachesTest: TestBaseEavDataSource
{
    public IListCacheSvc GetTestListCache() => GetService<IListCacheSvc>(); // new ListCache(new Log("test"));

    [TestMethod]
    public void QuickCache_AddListAndCheckIfIn()
    {
        const string ItemToFilter = "1023";
        var ds = CreateFilterForTesting(100, ItemToFilter);

        //var cache = ds.Root;
        var listCache = GetTestListCache();
        Assert.IsFalse(listCache.HasTA(ds.CacheFullKey), "Should not have it in cache yet");

        // manually add to cache
        listCache.Set(ds[DataSourceConstants.StreamDefaultName]);
        Assert.IsTrue(listCache.HasTA(ds.CacheFullKey + "&Stream=Default"), "Should have it in cache now");
        Assert.IsTrue(listCache.HasTA(ds[DataSourceConstants.StreamDefaultName]), "Should also have the DS default");
            
        Assert.IsTrue(listCache.HasTA(ds[DataSourceConstants.StreamDefaultName]), "should have it by stream as well");
            

        // Try to auto-retrieve 
        var cached = listCache.GetTA(ds.CacheFullKey + "&Stream=Default").List;

        Assert.AreEqual(1, cached.Count);

        cached = listCache.GetTA(ds[DataSourceConstants.StreamDefaultName]).List;
        Assert.AreEqual(1, cached.Count);

        var lci = listCache.GetTA(ds.CacheFullKey);
        Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");

        lci = listCache.GetTA(ds[DataSourceConstants.StreamDefaultName]);
        Assert.AreNotEqual(null, lci, "Cached should be found because using stream instead of name");

        cached = listCache.GetTA(ds[DataSourceConstants.StreamDefaultName]).List;
        Assert.AreEqual(1, cached.Count());

    }

    [TestMethod]
    public void QuickCache_AddAndWaitForReTimeout()
    {
        const string ItemToFilter = "1027";
        var ds = CreateFilterForTesting(100, ItemToFilter);

        var listCache = GetTestListCache();
        //listCache.DefaultDuration = 1;
        Assert.IsFalse(listCache.HasTA(ds.CacheFullKey), "Should not have it in cache yet");

        listCache.Set(ds.CacheFullKey, ds.ListForTests().ToImmutableList(), ds.CacheTimestamp, false, durationInSeconds: 1);
        Assert.IsTrue(listCache.HasTA(ds.CacheFullKey), "Should have it in cache now");

        Thread.Sleep(400);
        Assert.IsTrue(listCache.HasTA(ds.CacheFullKey), "Should STILL be in cache");

        Thread.Sleep(601);
        Assert.IsFalse(listCache.HasTA(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
    }




    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
        var filtered = CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachForTests(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}