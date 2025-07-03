#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.DataSource.Sys.AppDataSources;

internal class AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : ServiceBase("Eav.AppDtaSrcLoadUnk"), IIsUnknown, IAppDataSourcesLoader
{
    public AppLocalDataSources CompileDynamicDataSources(int appId) 
        => new([], new(), [], []);
}