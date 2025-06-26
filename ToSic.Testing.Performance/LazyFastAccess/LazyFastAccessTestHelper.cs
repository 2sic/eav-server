using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Testing.Performance.LoadPresetApp;

namespace ToSic.Testing.Performance.LazyFastAccess;
internal class LazyFastAccessTestHelper
{
    internal static (IAppStateCache AppState, IEnumerable<IEntity> List) LoadAppAndGetList()
    {
        var serviceProvider = Program.SetupServiceProvider();
        serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();
        var loader = serviceProvider.GetRequiredService<TestLoadPresetApp>();
        var appState = loader.Run();
        return (appState, appState.List);
    }

}
