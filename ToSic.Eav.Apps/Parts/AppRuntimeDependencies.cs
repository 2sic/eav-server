using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Parts
{
    [PrivateApi]
    public class AppRuntimeDependencies
    {
        public DataSourceFactory DataSourceFactory { get; }
        public IAppStates AppStates { get; }
        public ZoneRuntime ZoneRuntime { get; }

        public AppRuntimeDependencies(DataSourceFactory dataSourceFactory, 
            IAppStates appStates, 
            ZoneRuntime zoneRuntime)
        {
            DataSourceFactory = dataSourceFactory;
            AppStates = appStates;
            ZoneRuntime = zoneRuntime;
        }

    }
}
