using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.StartupTests;

namespace ToSic.Eav.DataSource.Caching;

[Startup(typeof(StartupTestsEavCoreAndDataSources))]
public class CacheAllStreamsTest(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
#if NETCOREAPP
    [field: AllowNull, MaybeNull]
#endif
    private CacheStreamsTestBuilder CacheStreamsTestBuilder => field ??= new(dsSvc, listCache, personTable);

    public const string AlternateIdForAlternateTests = "1069";


    // TODO: TEST INCORRECT - SHOULD ADD TEST1 FIRST
    [Fact]
    public void CheckInWithDifferentNewSetup()
    {
        var (filtered, _) = CacheStreamsTestBuilder.CreateFilterAndCache(100, AlternateIdForAlternateTests);

        False(listCache.HasStreamTac(filtered.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added different item");

    }
    
    [Fact]
    public void CheckInWithLongerChain()
    {
        const string FilterIdForManyTests = "1067";

        var filtered =CacheStreamsTestBuilder.CreateFilterForTesting(100, FilterIdForManyTests);
        var secondFilter = dsSvc.CreateDataSource<EntityTypeFilter>(filtered.Configuration.LookUpEngine);
        secondFilter.AttachTac(filtered);
        secondFilter.TypeName = "Person";

        Equal("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1067>EntityTypeFilter:NoGuid&TypeName=Person", secondFilter.CacheFullKey);

        False(listCache.HasStreamTac(secondFilter.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");
    }

    [Fact]
    public void CheckSourceExpiry()
    {
        var uniqueIdsForThisTest = "1001,1005,1043";
        var filtered =CacheStreamsTestBuilder.CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var query =CacheStreamsTestBuilder.CreateCacheDs(filtered);

        // shouldn't be in cache et...
        False(listCache.HasStreamTac(query.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in because the previous test added a shorter key");

        // Get first list from direct query and from cache - compare. Should be same
        var results1 = query[DataSourceConstants.StreamDefaultName].ListTac();
        var results2 = listCache.GetTac(query.Out[DataSourceConstants.StreamDefaultName]).List;
        Equal(results2, results1);//, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData =CacheStreamsTestBuilder.CreateFilterForTesting(100, uniqueIdsForThisTest, false);
        var cache2 =CacheStreamsTestBuilder.CreateCacheDs(laterTimeIdenticalData);
        var listFromCache2 = cache2[DataSourceConstants.StreamDefaultName].ListTac();
        NotEqual(listFromCache2, results1); //, "Second list shouldn't be same because 100ms time difference in source");
    }

    [Fact]
    public void IgnoreSourceExpiry()
    {
        const string idsForThisTest = "1001,1005,1043,1099";
        var filtered =CacheStreamsTestBuilder.CreateFilterForTesting(100, idsForThisTest);
        var cacheAllDs1 =CacheStreamsTestBuilder.CreateCacheDs(filtered);

        // Get first list from direct query and from cache - compare. Should be same
        var result = cacheAllDs1[DataSourceConstants.StreamDefaultName].ListTac();

        // Get result from Cache1 - which should match the result
        var cacheItem1 = listCache.GetTac(cacheAllDs1.Out[DataSourceConstants.StreamDefaultName]);
        NotNull(cacheItem1);//, "should be not null, expected it to be in the cache");
        var cacheItem1List = cacheItem1.List;
        Equal(cacheItem1List, result);//, "Should be same list - right now");

        // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(100);

        var laterTimeIdenticalData =CacheStreamsTestBuilder.CreateFilterForTesting(100, idsForThisTest);
        var cacheAllDs2 =CacheStreamsTestBuilder.CreateCacheDs(laterTimeIdenticalData, options: new
        {
            // special: tell cache2 to ignore time etc.
            RefreshOnSourceRefresh = false
        });

        // special: tell cache2 to ignore time etc.
        //cache2.RefreshOnSourceRefresh = false;
        Trace.WriteLine("Refresh..." + cacheAllDs2.RefreshOnSourceRefresh);

        // Ensure that the cache keys are identical
        Equal(cacheAllDs1.CacheFullKey, cacheAllDs2.CacheFullKey);//, "Cache keys should be identical");

        // Try to get the list from the second instance / cache
        // It should still be the same as the first, since all the specs are the same
        var listFromCache2 = cacheAllDs2[DataSourceConstants.StreamDefaultName].ListTac();
        Equal(listFromCache2, result);//, "Second list shouldn't STILL be same because we ignore time difference in source");
    }


    [Fact]
    public void ExpireBecauseOfTime()
    {
        var uniqueIdsForThisTest = "1001,1043,1099";
        var filtered =CacheStreamsTestBuilder.CreateFilterForTesting(100, uniqueIdsForThisTest);
        var cacheDs =CacheStreamsTestBuilder.CreateCacheDs(filtered, options: new
        {
            CacheDurationInSeconds = 1
        });

        // Get first list from direct query and from cache - compare. Should be same
        var originalList = cacheDs[DataSourceConstants.StreamDefaultName].ListTac();
        var listFromCache1 = listCache.GetTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]).List;
        Equal(listFromCache1, originalList);//, "Should be same list - right now");

        // Now wait 1 second, then repeat the process. Since the new source has another date-time, it should rebuild the cache
        Thread.Sleep(1001);

        // Create new identical filtered, and new cache-object to separate from original...
        filtered =CacheStreamsTestBuilder.CreateFilterForTesting(100, uniqueIdsForThisTest);
        var newCacheDs =CacheStreamsTestBuilder.CreateCacheDs(filtered, options: new
        {
            CacheDurationInSeconds = 1,
            RefreshOnSourceRefresh = false, // don't enforce this, otherwise it will automatically be a new cache anyhow
        });

        var listFromCacheAfter1Second = newCacheDs[DataSourceConstants.StreamDefaultName].ListTac();
        NotStrictEqual(listFromCacheAfter1Second, originalList);//, "Second list MUST be Different because 1 second passed");
    }
}