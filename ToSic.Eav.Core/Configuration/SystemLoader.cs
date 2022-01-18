using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class SystemLoader: HasLog
    {

        #region Constructor / DI

        public SystemLoader(IFingerprint fingerprint, IRuntime runtime, IAppsCache appsCache, IFeaturesInternal features, LogHistory logHistory) : base($"{LogNames.Eav}SysLdr")
        {
            _appsCache = appsCache;
            Features = features;
            _logHistory = logHistory;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            _fingerprint = fingerprint;
            _appStateLoader = runtime.Init(Log);
        }

        private readonly IFingerprint _fingerprint;
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
            LoadPresetApp();

            // V13 - Load Licenses
            // Avoid using DI, as otherwise someone could inject a different license loader
            new LicenseLoader(_appsCache, _fingerprint, _logHistory, Log).LoadLicenses();
            

            // Now do a normal reload of configuration and features
            LoadFeatures();
        }

        /// <summary>
        /// 2021-11-16 2dm - experimental, working on moving global/preset data into a normal AppState #PresetInAppState
        /// </summary>
        private void LoadPresetApp()
        {
            var wrapLog = Log.Call();
            Log.Add("Try to load global app-state");
            var appState = _appStateLoader.LoadFullAppState();
            _appsCache.Add(appState);
            wrapLog("ok");
        }

        private bool _startupAlreadyRan;

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        public void LoadFeatures()
        {
            var wrapLog = Log.Call();
            Features.Stored = new FeaturesLoader(_appsCache, _fingerprint, _logHistory, Log).LoadFeatures();
            Features.CacheTimestamp = DateTime.Now.Ticks;
            wrapLog("ok");
        }

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void ReloadFeatures()
        {
            _appStateLoader.UpdateConfig();
            LoadFeatures();
        }
    }
}
