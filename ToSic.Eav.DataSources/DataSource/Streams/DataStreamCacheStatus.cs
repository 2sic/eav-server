using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.Caching;
using ToSic.Eav.DataSource.Caching.CacheInfo;

namespace ToSic.Eav.DataSource.Streams;

/// <summary>
/// Provides information of how to cache a stream.
/// </summary>
public class DataStreamCacheStatus: CacheKey, ICacheExpiring
{
    public DataStreamCacheStatus(ICacheInfo cacheInfoProvider, IDataSource dataSource, string cacheSuffix) 
        : base(dataSource)
    {
        _cacheInfoProvider = cacheInfoProvider;
        _cacheSuffix = cacheSuffix;
    }

    private readonly ICacheInfo _cacheInfoProvider;
    private readonly string _cacheSuffix;

    public override string CacheFullKey => _cacheInfoProvider.CacheFullKey + "&Stream=" + _cacheSuffix;

    public long CacheTimestamp => _cacheInfoProvider.CacheTimestamp;

    public bool CacheChanged(long dependentTimeStamp) => DataSource.CacheChanged(dependentTimeStamp);
}