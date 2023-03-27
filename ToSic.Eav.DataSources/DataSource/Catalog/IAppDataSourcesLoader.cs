using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.DataSources.Catalog
{
    public interface IAppDataSourcesLoader
    {
        (List<DataSourceInfo> data, CacheItemPolicy policy) CreateAndReturnAppCache(int appId);
    }
}