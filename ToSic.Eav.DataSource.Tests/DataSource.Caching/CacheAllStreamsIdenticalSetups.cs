using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSource.Caching;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class CacheAllStreamsIdenticalSetups(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
#if NETCOREAPP
    [field: AllowNull, MaybeNull]
#endif
    private CacheStreamsTestBuilder CacheStreamsTestBuilder => field ??= new(dsSvc, listCache, personTable);

    /// <summary>
    /// This ID must be unique to this cache-test, other tests must use another number!
    /// </summary>
    private const string FilterIdForManyTests = "1072";
    public const int FilterIdForManyTestsInt = 1072;

    [Fact]
    public void Check2InWithIdenticalNewSetup()
    {
        var (filtered, cacheDs1) = CacheStreamsTestBuilder.CreateFilterAndCache(100, FilterIdForManyTests);

        // check if in cache - shouldn't be yet
        False(listCache.HasStreamTac(cacheDs1.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in yet");

        var resultSource1 = cacheDs1.ListTac(); // now it should get in

        // check again, should be in
        True(listCache.HasStreamTac(cacheDs1.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in now");

        // Result should be just 1
        Single(resultSource1);



        var (_, cacheDs2) = CacheStreamsTestBuilder.CreateFilterAndCache(100, FilterIdForManyTests);

        // Should already be in - even though it may be an old copy
        True(listCache.HasStreamTac(cacheDs2.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in because the previous test already added it - will fail if run by itself");

        var resultSource2 = cacheDs2.ListTac(); // now it should get in

        // var cnt = resultSource2.Count;
        // Should use Assert.Single, but for some reason this fails ???
        //Single(resultSource2, "still has correct amount of items");
#pragma warning disable xUnit2013
        Equal(1, resultSource2.Count()); //, "Should be in now");
#pragma warning restore xUnit2013
        Equal(FilterIdForManyTestsInt, resultSource2.First().EntityId); //, "check correct entity id");
    }

}