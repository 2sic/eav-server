using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Sys.Loaders;
using ToSic.Eav.Internal.Loaders;
using ToSic.Sys.Boot;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.StartUp;

/// <summary>
/// Load the preset app.
/// </summary>
internal class EavBootLoadPresetApp(
    ILogStore logStore,
    // loader should be lazy, so we can swap the log before it's created
    LazySvc<IAppStateLoader> appLoader,
    AppsCacheSwitch appsCache,
    EavFeaturesLoader featuresLoader)
    : BootProcessBase("SqlWUp", bootPhase: BootPhase.Loading, priority: LoadAppPriority, connect: [logStore, appLoader, appsCache, featuresLoader])
{
    public const int LoadAppPriority = 100;

    /// <summary>
    /// This will access the DB once to warm up the connection pool and make sure the DB is available.
    /// </summary>
    public override void Run() => LoadPresetApp();

    private void LoadPresetApp()
    {
        var (main, lStandalone, lNormal) = BootLogHelper.GetLoggersForStandaloneLogs(logStore, Log, "AppPst", "Load Global Preset App");
        var logSettings = new EavFeaturesLogSettingsHelper(featuresLoader, Log)
            .GetLogSettings();

        // Build the cache of all system-types. Must happen before everything else
        // This should use the lazy AppLoader, because the features should be loaded before it's created
        var presetApp = appLoader.LinkLog(main).Value.LoadFullAppState(logSettings);
        appsCache.Value.Add(presetApp.AppState);
        lStandalone.Done();
        lNormal.Done();
    }

}