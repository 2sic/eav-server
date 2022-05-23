using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EavSystemLoader : LoaderBase
    {
        #region Constructor / DI

        public EavSystemLoader(
            SystemFingerprint fingerprint, 
            IRuntime runtime, 
            Lazy<IGlobalConfiguration> globalConfiguration, 
            AppsCacheSwitch appsCache, 
            IFeaturesInternal features, 
            FeatureConfigManager featureConfigManager, 
            LicenseCatalog licenseCatalog, 
            LogHistory logHistory
        ) : base(logHistory, null, $"{LogNames.Eav}SysLdr", "System Load")
        {
            Fingerprint = fingerprint;
            _globalConfiguration = globalConfiguration;
            _appsCache = appsCache;
            _logHistory = logHistory;
            _appStateLoader = runtime.Init(Log);
            Features = features;
            _featureConfigManager = featureConfigManager;
            _licenseCatalog = licenseCatalog;
        }
        public SystemFingerprint Fingerprint { get; }
        private readonly IRuntime _appStateLoader;
        private readonly Lazy<IGlobalConfiguration> _globalConfiguration;
        private readonly AppsCacheSwitch _appsCache;
        public readonly IFeaturesInternal Features;
        private readonly FeatureConfigManager _featureConfigManager;
        private readonly LicenseCatalog _licenseCatalog;
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
            _logHistory.Add(LogNames.LogHistoryGlobalAndStartUp, assemblyLoadLog);
            AssemblyHandling.GetTypes(assemblyLoadLog);

            // Build the cache of all system-types. Must happen before everything else
            Log.A("Try to load global app-state");
            var presetApp = _appStateLoader.LoadFullAppState();
            _appsCache.Value.Add(presetApp);

            StartUpFeatures();
        }

        /// <summary>
        /// Standalone Features loading - to make the features API available in tests
        /// </summary>
        public void StartUpFeatures()
        {
            // V13 - Load Licenses
            // Avoid using DI, as otherwise someone could inject a different license loader
            new LicenseLoader(_logHistory, _licenseCatalog, Log)
                .LoadLicenses(Fingerprint.GetFingerprint(), _globalConfiguration.Value.GlobalFolder);

            // Now do a normal reload of configuration and features
            ReloadFeatures();
        }
        private bool _startupAlreadyRan;

        /// <summary>
        /// Reset the features stored by loading from 'features.json'.
        /// </summary>
        [PrivateApi]
        public bool ReloadFeatures() => SetFeaturesStored(LoadFeaturesStored());


        private bool SetFeaturesStored(FeatureListStored stored = null)
        {
            Features.Stored = stored ?? new FeatureListStored();
            Features.CacheTimestamp = DateTime.Now.Ticks;
            return true;
        }

        
        /// <summary>
        /// Load features stored from 'features.json'.
        /// When old format is detected, it is converted to new format.
        /// </summary>
        /// <returns></returns>
        private FeatureListStored LoadFeaturesStored()
        {
            var wrapLog = Log.Call<FeatureListStored>();

            try
            {
                var (filePath, fileContent) = _featureConfigManager.LoadFeaturesFile();
                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileContent)) 
                    return wrapLog("ok, but 'features.json' is missing", null);

                // handle old 'features.json' format
                var stored = _featureConfigManager.ConvertOldFeaturesFile(filePath, fileContent);
                if (stored != null) 
                    return wrapLog("converted to new features.json", stored);

                // return features stored
                return wrapLog("ok, features loaded", JsonConvert.DeserializeObject<FeatureListStored>(fileContent));
            }
            catch (Exception e)
            {
                Log.Ex(e);
                return wrapLog("load feature failed:" + e.Message, null);
            }
        }


        /// <summary>
        /// Update existing features config in "features.json". 
        /// </summary>
        [PrivateApi]
        public bool UpdateFeatures(List<FeatureManagementChange> changes)
        {
            var wrapLog = Log.Call<bool>($"c:{changes?.Count ?? -1}");
            var saved = _featureConfigManager.SaveFeaturesUpdate(changes);
            SetFeaturesStored(FeatureListStoredBuilder(changes));
            return wrapLog("ok, updated", saved);
        }


        private FeatureListStored FeatureListStoredBuilder(List<FeatureManagementChange> changes)
        {
            var updatedIds = changes.Select(f => f.FeatureGuid);

            var storedFeaturesButNotUpdated = Features.All
                .Where(f => f.EnabledStored.HasValue && !updatedIds.Contains(f.Guid))
                .Select(FeatureConfigManager.FeatureConfigBuilder).ToList();
            
            var updatedFeatures = changes
                .Where(f => f.Enabled.HasValue)
                .Select(FeatureConfigManager.FeatureConfigBuilder).ToList();

            return new FeatureListStored
            {
                Features = storedFeaturesButNotUpdated.Union(updatedFeatures).ToList(),
                Fingerprint = Fingerprint.GetFingerprint()
            };
        }
    }
}
