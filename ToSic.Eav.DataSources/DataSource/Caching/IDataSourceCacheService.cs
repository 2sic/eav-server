using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.Caching
{
    [PrivateApi]
    public interface IDataSourceCacheService
    {
        IListCacheSvc ListCache { get; }
        bool UnCache(int recursion, IDataSource dataSource, bool cascade = false, IReadOnlyDictionary<string, IDataStream> streams = default);
        bool UnCache(int recursion, IDataStream stream, bool cascade = false);
    }
}