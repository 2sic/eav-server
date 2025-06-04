using ToSic.Eav.DataSource.VisualQuery.Internal;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

internal class AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : ServiceBase("Eav.AppDtaSrcLoadUnk"), IIsUnknown, IAppDataSourcesLoader
{
    public (List<DataSourceInfo> data, TimeSpan slidingExpiration, IList<string> folderPaths, IEnumerable<string> cacheKeys) CompileDynamicDataSources(int appId) 
        => ([], new(), null, null);
}