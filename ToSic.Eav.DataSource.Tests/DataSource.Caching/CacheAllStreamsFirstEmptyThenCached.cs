using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSource.Caching;

/// <summary>
/// This test needs to be in an own class, to avoid the cache from being affected by other tests
/// </summary>
/// <param name="dsSvc"></param>
/// <param name="listCache"></param>
/// <param name="personTable"></param>
[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class CacheAllStreamsFirstEmptyThenCached(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
#if NETCOREAPP
    [field: AllowNull, MaybeNull]
#endif
    private CacheStreamsTestBuilder CacheStreamsTestBuilder => field ??= new(dsSvc, listCache, personTable);

    /// <summary>
    /// This ID must be unique to this cache-test, other tests must use another number!
    /// </summary>
    private const string FilterIdForThisTestOnly = "1042";

    private string ExpectedCacheId = "DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=" + FilterIdForThisTestOnly;

    [Fact]
    public void CheckNotInBeforeAndInAfterwards()
    {
        var (filtered, cacheDs) = CacheStreamsTestBuilder.CreateFilterAndCache(100, FilterIdForThisTestOnly);

        Equal(ExpectedCacheId, filtered.CacheFullKey);

        // check if in cache - shouldn't be yet
        False(listCache.HasStreamTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in yet");

        var y = cacheDs.ListTac(); // now it should get in

        // check again, should be in
        True(listCache.HasStreamTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]),
            "Should be in now");

        // Result should be just 1
        Single(y);
    }


}