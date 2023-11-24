using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.DataSource.Catalog;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppDataSourcesLoader
{
    (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId);
}