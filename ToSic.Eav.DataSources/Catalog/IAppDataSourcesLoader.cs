using System.Collections.Generic;
using System.Runtime.Caching;

namespace ToSic.Eav.DataSources.Catalog
{
    public interface IAppDataSourcesLoader
    {
        (List<DataSourceInfo> data, CacheItemPolicy policy) CreateAndReturnAppCache(int appId);
    }
}