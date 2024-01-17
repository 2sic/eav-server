using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSource.Streams.Internal;

/// <summary>
/// Provides information of how to cache a stream.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataStreamCacheStatus(ICacheInfo cacheInfoProvider, IDataSource dataSource, string cacheSuffix)
    : CacheKey(dataSource), ICacheExpiring
{
    public override string CacheFullKey => cacheInfoProvider.CacheFullKey + "&Stream=" + cacheSuffix;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public long CacheTimestamp => cacheInfoProvider.CacheTimestamp;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public bool CacheChanged(long dependentTimeStamp) => DataSource.CacheChanged(dependentTimeStamp);
}