using System;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.DataSources
{
    public class AppDataSourcesLoaderUnknown : ServiceBase, IIsUnknown, IAppDataSourcesLoader
    {
        public AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : base("Eav.AppDtaSrcLoadUnk")
        { }

        public void Register(AppState appState)
        {
            throw new NotImplementedException();
        }
    }
}
