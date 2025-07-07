using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Streams;

/// <summary>
/// Provides information of how to cache a stream.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataStreamCacheStatus(ICacheInfo cacheInfoProvider, IDataSource dataSource, string cacheSuffix)
    : CacheKey(dataSource), ICacheExpiring
{
    public override string CacheFullKey => cacheInfoProvider.CacheFullKey + "&Stream=" + cacheSuffix;

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public long CacheTimestamp => cacheInfoProvider.CacheTimestamp;

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public bool CacheChanged(long dependentTimeStamp) => DataSource.CacheChanged(dependentTimeStamp);
}