using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.Caching;

[PrivateApi]
public interface IDataSourceCacheService
{
    IListCacheSvc ListCache { get; }

    bool FlushAll();

    bool Flush(string key);

    bool Flush(IDataSource dataSource, bool cascade = false);
    bool Flush(IDataStream stream, bool cascade = false);
}