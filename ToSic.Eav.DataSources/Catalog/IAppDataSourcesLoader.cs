using System.Collections.Generic;
using ToSic.Eav.Apps;

namespace ToSic.Eav.DataSources.Catalog
{
    public interface IAppDataSourcesLoader
    {
        List<DataSourceInfo> Get(int appId);
    }
}