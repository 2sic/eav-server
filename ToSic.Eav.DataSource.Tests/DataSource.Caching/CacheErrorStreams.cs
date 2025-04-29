using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSource.Internal.Errors;

namespace ToSic.Eav.DataSource.Caching;

/// <summary>
/// This test needs to be in an own class, to avoid the cache from being affected by other tests
/// </summary>
/// <param name="dsSvc"></param>
/// <param name="listCache"></param>
/// <param name="personTable"></param>
[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class CacheErrorStreams(DataSourcesTstBuilder dsSvc, IListCacheSvc listCache, DataTablePerson personTable)
{
#if NETCOREAPP
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
    private CacheStreamsTestBuilder CacheStreamsTestBuilder => field ??= new(dsSvc, listCache, personTable);

    private const string ExpectedCacheId = "Error:NoGuid";

    [Theory]
    [InlineData(0, 0, 0, false)]
    [InlineData(1, 0, 0, true)]
    [InlineData(0, 1, 0, true)]
    [InlineData(0, 1, 3, false)]
    [InlineData(1, 2, 0, true)]
    public void CheckNotInBeforeAndInAfterwards(int errCacheDuration, int delayData, int delayTest, bool expected)
    {
        var (_, cacheDs) = CacheStreamsTestBuilder.CreateErrorAndCache(errCacheDuration, delayData);

        // check if in cache - shouldn't be yet
        False(listCache.HasStreamTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]),
            "Should not be in yet");

        // Access to get into cache
        cacheDs.ListTac();

        // wait for the error to be generated
        if (delayTest > 0)
            Thread.Sleep(delayTest * 1000);

        // check again, should be in
        if (expected)
            True(listCache.HasStreamTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]), "expected to be in cache");
        else
            False(listCache.HasStreamTac(cacheDs.Out[DataSourceConstants.StreamDefaultName]), "expected to be NOT in cache");

        // Clean up to avoid side effect on following test
        ((ListCacheSvc)listCache).Remove(cacheDs.Out[DataSourceConstants.StreamDefaultName]);
    }

    [Fact]
    public void StandaloneErrorCacheKey()
        => Equal(ExpectedCacheId, CacheStreamsTestBuilder.CreateErrorAndCache(0, 0).Source.CacheFullKey);

    [Fact]
    public void ErrorCacheHas1Item()
        => Single(CacheStreamsTestBuilder.CreateErrorAndCache(0, 0).Cache.ListTac());

    [Fact]
    public void ErrorCacheItemIsError()
        => True(CacheStreamsTestBuilder.CreateErrorAndCache(0, 0).Cache.IsError(), "should be error");

}