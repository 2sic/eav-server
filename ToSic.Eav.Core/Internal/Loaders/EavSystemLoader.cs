using ToSic.Eav.Caching;
using ToSic.Eav.Plumbing;
using ToSic.Eav.StartUp;
using ToSic.Lib.DI;

namespace ToSic.Eav.Internal.Loaders;

[PrivateApi]
public class EavSystemLoader(LazySvc<IAppLoader> appLoader, AppsCacheSwitch appsCache, ILogStore logStore, EavFeaturesLoader featuresLoader)
    : LoaderBase(logStore, $"{EavLogs.Eav}SysLdr", connect: [appsCache, logStore, appLoader, featuresLoader])
{
    private readonly ILogStore _logStore = logStore;
    private bool _startupAlreadyRan;

    /// <summary>
    /// Do things we need at application start
    /// </summary>
    public void StartUp()
    {
        var bl = BootLog.Log.Fn("Eav: StartUp", timer: true);
        // Prevent multiple Initializations
        if (_startupAlreadyRan)
            throw new("Startup should never be called twice.");
        _startupAlreadyRan = true;

        // Pre-Load the Assembly list into memory to log separately
        var assemblyLoadLog = new Log(EavLogs.Eav + "AssLdr", null, "Load Assemblies");
        _logStore.Add(LogNames.LogStoreStartUp, assemblyLoadLog);

        var l = Log.Fn(timer: true);
        AssemblyHandling.GetTypes(assemblyLoadLog);

        var logSettings = new AppLoaderLoggingHelper(featuresLoader).GetLogSettings();

        // Build the cache of all system-types. Must happen before everything else
        // This should use the lazy AppLoader, because the features should be loaded before it's created
        l.A("Try to load global app-state");
        var presetApp = appLoader.Value.LoadFullAppState(logSettings);
        appsCache.Value.Add(presetApp.AppState);

        featuresLoader.LoadLicenseAndFeatures();
        bl.Done();
        l.Done("ok");
    }

}