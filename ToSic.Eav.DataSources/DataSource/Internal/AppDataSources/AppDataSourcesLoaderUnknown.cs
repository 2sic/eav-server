using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSource.VisualQuery.Internal;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

internal class AppDataSourcesLoaderUnknown : ServiceBase, IIsUnknown, IAppDataSourcesLoader
{
    public AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : base("Eav.AppDtaSrcLoadUnk")
    { }

    public (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId) 
        => (new List<DataSourceInfo>(), new CacheItemPolicy());
}