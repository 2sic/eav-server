using ToSic.Eav.DataSources;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    [PrivateApi]
    public class AppRuntimeDependencies: ServiceDependencies
    {
        public DataSourceFactory DataSourceFactory { get; }
        public IAppStates AppStates { get; }
        public ZoneRuntime ZoneRuntime { get; }

        public AppRuntimeDependencies(
            DataSourceFactory dataSourceFactory,
            IAppStates appStates,
            ZoneRuntime zoneRuntime
        ) => AddToLogQueue(
            DataSourceFactory = dataSourceFactory,
            AppStates = appStates,
            ZoneRuntime = zoneRuntime
        );
    }
}
