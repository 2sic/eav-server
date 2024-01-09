using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppDataSourcesLoader
{
    (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId);
}