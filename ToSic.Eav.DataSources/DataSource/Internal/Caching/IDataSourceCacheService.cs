namespace ToSic.Eav.DataSource.Internal.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDataSourceCacheService
{
    IListCacheSvc ListCache { get; }

    bool FlushAll();

    bool Flush(string key);

    bool Flush(IDataSource dataSource, bool cascade = false);
    bool Flush(IDataStream stream, bool cascade = false);
}