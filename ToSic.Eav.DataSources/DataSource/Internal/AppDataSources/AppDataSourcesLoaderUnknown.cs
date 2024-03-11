using System.Runtime.Caching;
using ToSic.Eav.DataSource.VisualQuery.Internal;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

internal class AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : ServiceBase("Eav.AppDtaSrcLoadUnk"), IIsUnknown, IAppDataSourcesLoader
{
    public (List<DataSourceInfo> data, CacheItemPolicy policy) CompileDynamicDataSources(int appId) 
        => ([], new());
}