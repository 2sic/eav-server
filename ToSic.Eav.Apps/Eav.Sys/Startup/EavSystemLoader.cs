using System.Runtime.CompilerServices;
using ToSic.Eav.Caching;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.StartUp;
using ToSic.Sys.Startup;

namespace ToSic.Eav.Internal.Loaders;

[PrivateApi]
public class EavSystemLoader(
    // loader should be lazy, so we can swap the log before it's created
    LazySvc<IAppLoader> appLoader,
    LazySvc<ITargetTypeService> typeSvc,
    AppsCacheSwitch appsCache,
    ILogStore logStore,
    EavFeaturesLoader featuresLoader)
    : LoaderBase(logStore, $"{EavLogs.Eav}SysLdr", connect: [appsCache, typeSvc, logStore, appLoader, featuresLoader])
{
    private static bool _startupAlreadyRan;

    /// <summary>
    /// Do things we need at application start
    /// </summary>
    public void StartUp()
    {
        var l = BootLog.Log.Fn("Eav: StartUp", timer: true);
        // Prevent multiple Initializations
        if (_startupAlreadyRan)
            throw new("Startup should never be called twice.");
        _startupAlreadyRan = true;

        // Pre-Load the Assembly list into memory to log separately
        WarmUpAssembliesIndex();

        // Do initial access to the DB so it's faster later
        WarmUpSql();

        // Load the preset app
        LoadPresetApp();

        // Finally, load all the licenses and features
        featuresLoader.LoadLicenseAndFeatures();

        l.Done("startup complete");
    }

    private (ILog Main, ILogCall lStandalone, ILogCall lNormal) GetLoggersForStandaloneLogs(string partName, string message, [CallerMemberName] string? cName = default)
    {
        // Pre-Load the Assembly list into memory to log separately
        var standaloneLog = new Log(EavLogs.Eav + partName, null, message);
        logStore.Add(LogNames.LogStoreStartUp, standaloneLog);
        var lStandalone = standaloneLog.Fn(timer: true, cName: cName);
        var lNormal = Log.Fn(timer: true, cName: cName);
        return (standaloneLog, lStandalone, lNormal);
    }

    private void WarmUpAssembliesIndex()
    {
        var (_, lStandalone, lNormal) = GetLoggersForStandaloneLogs("AssLdr", "Load Assemblies");
        AssemblyHandling.GetTypes(lStandalone);
        lStandalone.Done();
        lNormal.Done();
    }

    /// <summary>
    /// This will access the DB once to warm up the connection pool and make sure the DB is available.
    /// </summary>
    private void WarmUpSql()
    {
        var (main, lStandalone, lNormal) = GetLoggersForStandaloneLogs("SqlWUp", "Warm up SQL");
        typeSvc.LinkLog(main).Value.GetName(1);
        lStandalone.Done();
        lNormal.Done();
    }

    private void LoadPresetApp()
    {
        var (main, lStandalone, lNormal) = GetLoggersForStandaloneLogs("AppPst", "Load Global Preset App");
        var logSettings = new EavSystemLoaderLogSettingsHelper(featuresLoader, Log)
            .GetLogSettings();

        // Build the cache of all system-types. Must happen before everything else
        // This should use the lazy AppLoader, because the features should be loaded before it's created
        var presetApp = appLoader.LinkLog(main).Value.LoadFullAppState(logSettings);
        appsCache.Value.Add(presetApp.AppState);
        lStandalone.Done();
        lNormal.Done();
    }
}