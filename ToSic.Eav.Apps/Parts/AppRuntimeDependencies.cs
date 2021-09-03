using System;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Parts
{
    [PrivateApi]
    public class AppRuntimeDependencies
    {
        public DataSourceFactory DataSourceFactory { get; }
        public IServiceProvider ServiceProvider { get; }
        public IAppStates AppStates { get; }
        public ZoneRuntime ZoneRuntime { get; }

        public AppRuntimeDependencies(DataSourceFactory dataSourceFactory, IServiceProvider serviceProvider, IAppStates appStates, ZoneRuntime zoneRuntime)
        {
            DataSourceFactory = dataSourceFactory;
            ServiceProvider = serviceProvider;
            AppStates = appStates;
            ZoneRuntime = zoneRuntime;
        }

    }
}
