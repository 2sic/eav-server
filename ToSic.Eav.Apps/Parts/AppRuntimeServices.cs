using ToSic.Eav.DataSources;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    [PrivateApi]
    public class AppRuntimeServices: MyServicesBase
    {
        public DataSourceFactory DataSourceFactory { get; }
        public IAppStates AppStates { get; }
        public ZoneRuntime ZoneRuntime { get; }

        public AppRuntimeServices(
            DataSourceFactory dataSourceFactory,
            IAppStates appStates,
            ZoneRuntime zoneRuntime
        ) => ConnectServices(
            DataSourceFactory = dataSourceFactory,
            AppStates = appStates,
            ZoneRuntime = zoneRuntime
        );
    }
}
