﻿using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Caches;

[TestClass]
public class CacheAllStreamsTest: TestBaseEavDataSource
{
    const string FilterIdForManyTests = "1067";
    const string AlternateIdForAlternateTests = "1069";
    public IListCacheSvc GetTestListCache() => GetService<IListCacheSvc>(); // new ListCache(new Log("test"));

    [TestMethod]
    public void CacheAllStreams_Check1NotInBeforeAndInAfterwards()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

        var cacher = CreateCacheDS(filtered);

        var listCache = GetTestListCache();

        Assert.AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1067", filtered.CacheFullKey);

        // check if in cache - shouldn't be yet
        Assert.IsFalse(listCache.HasTA(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in yet");

        var y = cacher.ListForTests(); // not it should get in

        // check again, should be in
        Assert.IsTrue(listCache.HasTA(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in now");
    }

    [TestMethod]
    public void CacheAllStreams_Check2InWithIdenticalNewSetup()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

        var cacher = CreateCacheDS(filtered);
        var listCache = GetTestListCache();

        // Should already be in - even though it may be an old copy
        Assert.IsTrue(listCache.HasTA(cacher.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in because the previous test already added it - will fail if run by itself");

        var y = cacher.ListForTests(); // not it should get in

        Assert.AreEqual(1, y.Count(), "still has correct amount of items");
        Assert.AreEqual(1067, y.First().EntityId, "check correct entity id");
    }

    [TestMethod]
    public void CacheAllStreams_CheckInWithDifferentNewSetup()
    {
        var filtered = CreateFilterForTesting(100, AlternateIdForAlternateTests);

        CreateCacheDS(filtered);
        var listCache = GetTestListCache();

        Assert.IsFalse(listCache.HasTA(filtered.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added different item");

    }

    private CacheAllStreams CreateCacheDS(IDataSource filtered, object options = default)
    {
        var cacher = CreateDataSourceNew<CacheAllStreams>(/*filtered.Configuration.LookUpEngine,*/ options);
        cacher.AttachForTests(filtered);
        return cacher;
    }

    [TestMethod]
    public void CacheAllStreams_CheckInWithLongerChain()
    {
        var filtered = CreateFilterForTesting(100, FilterIdForManyTests);
        var secondFilter = CreateDataSource<EntityTypeFilter>(filtered.Configuration.LookUpEngine);
        secondFilter.AttachForTests(filtered);
        secondFilter.TypeName = "Person";

        var cacher = CreateCacheDS(secondFilter);
        var listCache = GetTestListCache();

        Assert.AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1067>EntityTypeFilter:NoGuid&TypeName=Person", secondFilter.CacheFullKey);

        Assert.IsFalse(listCache.HasTA(secondFilter.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");
    }

    [TestMethod]
    public void CacheAllStreams_CheckSourceExpiry()
    {
        var uniqueIdsForThisTest = "1001,1005,1043";
        var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var query = CreateCacheDS(filtered);
        var listCache = GetTestListCache();

        // shouldn't be in cache et...
        Assert.IsFalse(listCache.HasTA(query.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");

        // Get first list from direct query and from cache - compare. Should be same
        var results1 = query[DataSourceConstants.StreamDefaultName].ListForTests();
        var results2 = listCache.GetTA(query.Out[DataSourceConstants.StreamDefaultName]).List; Assert.AreEqual(results2, results1, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData = CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var cache2 = CreateCacheDS(laterTimeIdenticalData);
        var listFromCache2 = cache2[DataSourceConstants.StreamDefaultName].ListForTests();
        Assert.AreNotEqual(listFromCache2, results1, "Second list shouldn't be same because 100ms time difference in source");
    }

    [TestMethod]
    public void CacheAllStreams_IgnoreSourceExpiry()
    {
        var uniqueIdsForThisTest = "1001,1005,1043,1099";
        var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
        var query = CreateCacheDS(filtered);
        var listCache = GetTestListCache();

        // Get first list from direct query and from cache - compare. Should be same
        var results1 = query[DataSourceConstants.StreamDefaultName].ListForTests();

        var cacheItem = listCache.GetTA(query.Out[DataSourceConstants.StreamDefaultName]);
        Assert.IsNotNull(cacheItem, "should be not null, expected it to be in the cache");
        var results2 = cacheItem.List;
        Assert.AreEqual(results2, results1, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData = CreateFilterForTesting(100, uniqueIdsForThisTest);
        var cache2 = CreateCacheDS(laterTimeIdenticalData, options: new
        {
            RefreshOnSourceRefresh = false
        });

        // special: tell cache2 to ignore time etc.
        //cache2.RefreshOnSourceRefresh = false;

        var listFromCache2 = cache2[DataSourceConstants.StreamDefaultName].ListForTests();
        Assert.AreEqual(listFromCache2, results1, "Second list sohuldn't STILL be same because we ignore time difference in source");
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
        var listCache = GetTestListCache();

        // Get first list from direct query and from cache - compare. Should be same
        var originalList = cacher[DataSourceConstants.StreamDefaultName].ListForTests();
        var listFromCache1 = listCache.GetTA(cacher.Out[DataSourceConstants.StreamDefaultName]).List;
        Assert.AreEqual(listFromCache1, originalList, "Should be same list - right now");

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
        var listFromCacheAfter1Second = newCacher[DataSourceConstants.StreamDefaultName].ListForTests();
        Assert.AreNotEqual(listFromCacheAfter1Second, originalList, "Second list MUST be Different because 1 second passed");
    }

    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue, bool useCacheForSpeed = true)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001, useCacheForSpeed);
        var filtered = CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachForTests(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}