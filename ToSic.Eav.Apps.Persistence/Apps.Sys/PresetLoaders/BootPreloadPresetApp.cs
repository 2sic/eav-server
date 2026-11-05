using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Sys.Boot;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

/// <summary>
/// Load the preset app from the file system during boot.
/// </summary>
internal sealed class BootPreloadPresetApp(
    ILogStore logStore,
    // loader should be lazy, so we can swap the log before it's created
    LazySvc<IAppStateLoader> appLoader,
    AppsCacheSwitch appsCache,
    EavFeaturesLoader featuresLoader)
    : ServiceBase("AppPst", connect: [logStore, appLoader, appsCache, featuresLoader]), IBootProcess
{
    public string NameId => Log.NameId;

    public BootPhase Phase => BootPhase.Loading;

    public int Priority => 100; // very early
    
    public void Run()
    {
        var (main, lStandalone, lNormal) = BootLogHelper
            .GetLoggersForStandaloneLogs(logStore, Log, "AppPst", "Load Global Preset App");
        var logSettings = new EavFeaturesLogSettingsHelper(featuresLoader, Log)
            .GetLogSettings();

        // Build the cache of all system-types. Must happen before everything else
        // This should use the lazy AppLoader, because the features should be loaded before it's created
        var presetApp = appLoader.LinkLog(main).Value
            .LoadFullAppState(logSettings);
        appsCache.Value.Add(presetApp.AppState);
        lStandalone.Done();
        lNormal.Done();
    }

}