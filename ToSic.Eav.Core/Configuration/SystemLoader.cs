using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class SystemLoader: LoaderBase
    {
        #region Constructor / DI

        public SystemLoader(SystemFingerprint fingerprint, IRuntime runtime, IAppsCache appsCache, IFeaturesInternal features, LogHistory logHistory) 
            : base(logHistory, null, $"{LogNames.Eav}SysLdr", "System Load")
        {
            Fingerprint = fingerprint;
            _appsCache = appsCache;
            _logHistory = logHistory;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            _appStateLoader = runtime.Init(Log);
            Features = features;
        }
        public SystemFingerprint Fingerprint { get; }
        private readonly IRuntime _appStateLoader;
        private readonly IAppsCache _appsCache;
        public readonly IFeaturesInternal Features;
        private readonly LogHistory _logHistory;

        public SystemLoader Init(ILog parentLog)
        {
            Log.LinkTo(parentLog);
            return this;
        }

        #endregion

        /// <summary>
        /// Do things needed at application start
        /// </summary>
        public void StartUp()
        {
            // Prevent multiple Inits
            if (_startupAlreadyRan) throw new Exception("Startup should never be called twice.");
            _startupAlreadyRan = true;

            // Pre-Load the Assembly list into memory to log separately
            var assemblyLoadLog = new Log(LogNames.Eav + "AssLdr", null, "Load Assemblies");
            _logHistory.Add(LogNames.LogHistoryGlobalTypes, assemblyLoadLog);
            AssemblyHandling.GetTypes(assemblyLoadLog);

            // Build the cache of all system-types. Must happen before everything else
            Log.Add("Try to load global app-state");
            var presetApp = _appStateLoader.LoadFullAppState();
            _appsCache.Add(presetApp);

            // V13 - Load Licenses
            // Avoid using DI, as otherwise someone could inject a different license loader
            new LicenseLoader(_logHistory, Log).Init(Fingerprint.GetFingerprint()).LoadLicenses(presetApp);

            // Now do a normal reload of configuration and features
            LoadFeatures(presetApp);
        }
        

        private bool _startupAlreadyRan;

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        public void LoadFeatures(AppState presetApp = null)
        {
            var wrapLog = Log.Call();
            presetApp = presetApp ?? _appsCache.Get(null, Constants.PresetIdentity);
            Features.Stored = new FeaturesLoader(_logHistory, Log).LoadFeatures(presetApp, Fingerprint.GetFingerprint());
            Features.CacheTimestamp = DateTime.Now.Ticks;
            wrapLog("ok");
        }

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void ReloadFeatures()
        {
            _appStateLoader.ReloadConfigEntities();
            LoadFeatures();
        }
    }
}
