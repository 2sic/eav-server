using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.DataSource.Catalog;

public interface IAppDataSourcesLoader
{
    (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId);
}