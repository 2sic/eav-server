using System.Diagnostics;
using System.Threading;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.TestData;

namespace ToSic.Eav.DataSourceTests.Caches;

[TestClass]
public class CacheAllStreamsTest: TestBaseEavDataSource
{
    private const string FilterIdForManyTests = "1067";
    private const string AlternateIdForAlternateTests = "1069";

    public IListCacheSvc GetListCacheSvc() => GetService<IListCacheSvc>();

    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void CacheAllStreams_Check1NotInBeforeAndInAfterwards()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

        var cacher = CreateCacheDS(filtered);

        var listCache = GetListCacheSvc();

        AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1067", filtered.CacheFullKey);

        // check if in cache - shouldn't be yet
        IsFalse(listCache.HasStreamTac(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in yet");

        var y = cacher.ListTac(); // now it should get in

        // check again, should be in
        IsTrue(listCache.HasStreamTac(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in now");
    }

    [TestMethod]
    public void CacheAllStreams_Check2InWithIdenticalNewSetup()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

        var cacher = CreateCacheDS(filtered);
        var listCache = GetListCacheSvc();

        // Should already be in - even though it may be an old copy
        IsTrue(listCache.HasStreamTac(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in because the previous test already added it - will fail if run by itself");

        var y = cacher.ListTac(); // not it should get in

        AreEqual(1, y.Count(), "still has correct amount of items");
        AreEqual(1067, y.First().EntityId, "check correct entity id");
    }

    [TestMethod]
    public void CacheAllStreams_CheckInWithDifferentNewSetup()
    {
        var filtered = CreateFilterForTesting(100, AlternateIdForAlternateTests);

        CreateCacheDS(filtered);
        var listCache = GetListCacheSvc();

        IsFalse(listCache.HasStreamTac(filtered.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added different item");

    }

    private CacheAllStreams CreateCacheDS(IDataSource filtered, object options = default)
    {
        var cacheAll = DsSvc.CreateDataSourceNew<CacheAllStreams>(options);
        cacheAll.AttachTac(filtered);
        return cacheAll;
    }

    [TestMethod]
    public void CacheAllStreams_CheckInWithLongerChain()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);
        var secondFilter = DsSvc.CreateDataSource<EntityTypeFilter>(filtered.Configuration.LookUpEngine);
        secondFilter.AttachTac(filtered);
        secondFilter.TypeName = "Person";

        var cacher = CreateCacheDS(secondFilter);
        var listCache = GetListCacheSvc();

        AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1067>EntityTypeFilter:NoGuid&TypeName=Person", secondFilter.CacheFullKey);

        IsFalse(listCache.HasStreamTac(secondFilter.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");
    }

    [TestMethod]
    public void CacheAllStreams_CheckSourceExpiry()
    {
        var uniqueIdsForThisTest = "1001,1005,1043";
        var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var query = CreateCacheDS(filtered);
        var listCache = GetListCacheSvc();

        // shouldn't be in cache et...
        IsFalse(listCache.HasStreamTac(query.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");

        // Get first list from direct query and from cache - compare. Should be same
        var results1 = query[DataSourceConstants.StreamDefaultName].ListTac();
        var results2 = listCache.GetTac(query.Out[DataSourceConstants.StreamDefaultName]).List;
        AreEqual(results2, results1, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData = CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var cache2 = CreateCacheDS(laterTimeIdenticalData);
        var listFromCache2 = cache2[DataSourceConstants.StreamDefaultName].ListTac();
        AreNotEqual(listFromCache2, results1, "Second list shouldn't be same because 100ms time difference in source");
    }

    [TestMethod]
    public void CacheAllStreams_IgnoreSourceExpiry()
    {
        const string idsForThisTest = "1001,1005,1043,1099";
        var filtered = CreateFilterForTesting(100, idsForThisTest);
        var cacheAllDs1 = CreateCacheDS(filtered);
        var listCacheSvc = GetListCacheSvc();

        // Get first list from direct query and from cache - compare. Should be same
        var result = cacheAllDs1[DataSourceConstants.StreamDefaultName].ListTac();

        // Get result from Cache1 - which should match the result
        var cacheItem1 = listCacheSvc.GetTac(cacheAllDs1.Out[DataSourceConstants.StreamDefaultName]);
        IsNotNull(cacheItem1, "should be not null, expected it to be in the cache");
        var cacheItem1List = cacheItem1.List;
        AreEqual(cacheItem1List, result, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData = CreateFilterForTesting(100, idsForThisTest);
        var cacheAllDs2 = CreateCacheDS(laterTimeIdenticalData, options: new
        {
            // special: tell cache2 to ignore time etc.
            RefreshOnSourceRefresh = false
        });

        // special: tell cache2 to ignore time etc.
        //cache2.RefreshOnSourceRefresh = false;
        Trace.WriteLine("Refresh..." + cacheAllDs2.RefreshOnSourceRefresh);

        // Ensure that the cache keys are identical
        AreEqual(cacheAllDs1.CacheFullKey, cacheAllDs2.CacheFullKey, "Cache keys should be identical");

        // Try to get the list from the second instance / cache
        // It should still be the same as the first, since all the specs are the same
        var listFromCache2 = cacheAllDs2[DataSourceConstants.StreamDefaultName].ListTac();
        AreEqual(listFromCache2, result, "Second list shouldn't STILL be same because we ignore time difference in source");
    }


    [TestMethod]
    public void CacheAllStreams_ExpireBecauseOfTime()
    {
        var uniqueIdsForThisTest = "1001,1043,1099";
        var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
        var cacher = CreateCacheDS(filtered, options: new
        {
            CacheDurationInSeconds = 1
        });
        //cacher.CacheDurationInSeconds = 1;
        var listCache = GetListCacheSvc();

        // Get first list from direct query and from cache - compare. Should be same
        var originalList = cacher[DataSourceConstants.StreamDefaultName].ListTac();
        var listFromCache1 = listCache.GetTac(cacher.Out[DataSourceConstants.StreamDefaultName]).List;
        AreEqual(listFromCache1, originalList, "Should be same list - right now");

        // Now wait 1 second, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(1001);

        // Create new identical filtered, and new cache-object to separate from original...
        filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
        var newCacher = CreateCacheDS(filtered, options: new
        {
            CacheDurationInSeconds = 1,
            RefreshOnSourceRefresh = false, // don't enforce this, otherwise it will automatically be a new cache anyhow
        });
        //newCacher.CacheDurationInSeconds = 1;
        //newCacher.RefreshOnSourceRefresh = false; // don't enforce this, otherwise it will automatically be a new cache anyhow
        var listFromCacheAfter1Second = newCacher[DataSourceConstants.StreamDefaultName].ListTac();
        AreNotEqual(listFromCacheAfter1Second, originalList, "Second list MUST be Different because 1 second passed");
    }

    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue, bool useCacheForSpeed = true)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001, useCacheForSpeed);
        var filtered = DsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachTac(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}