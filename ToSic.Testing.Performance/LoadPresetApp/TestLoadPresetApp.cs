using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Lib.Logging;

namespace ToSic.Testing.Performance.LoadPresetApp;
internal class TestLoadPresetApp(IAppStateLoader appLoader)
{
    
    public IAppStateCache Run()
    {
        var logSettings = new LogSettings(false, false, false);
        // This will access the DB once to warm up the connection pool and make sure the DB is available.
        var appState = appLoader.LoadFullAppState(logSettings);
        // Do something with appState if needed, e.g. log it or validate it
        return appState.AppState;
    }
}
