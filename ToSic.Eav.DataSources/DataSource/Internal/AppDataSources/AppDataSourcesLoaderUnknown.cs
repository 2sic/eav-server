using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery.Internal;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

internal class AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : ServiceBase("Eav.AppDtaSrcLoadUnk"), IIsUnknown, IAppDataSourcesLoader
{
    public (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId) 
        => ([], new CacheItemPolicy());
}