using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using ToSic.Eav.Caching.CachingMonitors;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        public List<DataSourceInfo> Get(int appId) => _appDataSourcesLoader.Value.Get(appId);
    }
}
