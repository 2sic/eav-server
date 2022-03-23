using System;
using System.IO;
using Newtonsoft.Json;
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

        public SystemLoader(SystemFingerprint fingerprint, IRuntime runtime, Lazy<IGlobalConfiguration> globalConfiguration, IAppsCache appsCache, IFeaturesInternal features, LogHistory logHistory) 
            : base(logHistory, null, $"{LogNames.Eav}SysLdr", "System Load")
        {
            Fingerprint = fingerprint;
            _globalConfiguration = globalConfiguration;
            _appsCache = appsCache;
            _logHistory = logHistory;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            _appStateLoader = runtime.Init(Log);
            Features = features;
        }
        public SystemFingerprint Fingerprint { get; }
        private readonly IRuntime _appStateLoader;
        private readonly Lazy<IGlobalConfiguration> _globalConfiguration;
        private readonly IAppsCache _appsCache;
        public readonly IFeaturesInternal Features;
        private readonly LogHistory _logHistory;

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

            StartUpFeatures();
        }

        /// <summary>
        /// Standalone Features loading - to make the features API available in tests
        /// </summary>
        public void StartUpFeatures()
        {
            // V13 - Load Licenses
            // Avoid using DI, as otherwise someone could inject a different license loader
            new LicenseLoader(_logHistory, Log).LoadLicenses(Fingerprint.GetFingerprint(),
                _globalConfiguration.Value.GlobalFolder);

            // Now do a normal reload of configuration and features
            ReloadFeatures();
        }


        private bool _startupAlreadyRan;

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void ReloadFeatures()
        {
            var wrapLog = Log.Call();
            var features = new FeatureListStored();

            // load features in simple way
            var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);

            var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);
            if (File.Exists(featureFilePath))
            {
                var featStr = File.ReadAllText(featureFilePath);
                features = JsonConvert.DeserializeObject<FeatureListStored>(featStr);
            }

            Features.Stored = features;
            Features.CacheTimestamp = DateTime.Now.Ticks;
            wrapLog("ok");
        }

    }
}
