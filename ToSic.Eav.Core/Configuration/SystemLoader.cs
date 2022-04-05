using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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
    public class SystemLoader : LoaderBase
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
        public bool ReloadFeatures()
        {
            var wrapLog = Log.Call<bool>();
            
            // set default (no features stored)
            Features.Stored = new FeatureListStored();
            Features.CacheTimestamp = DateTime.Now.Ticks;

            // folder with "features.json"
            var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);
            
            var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);
            
            if (!File.Exists(featureFilePath)) return wrapLog("ok, but 'features.json' is missing", true);

            try
            {
                var featStr = File.ReadAllText(featureFilePath);

                // check json format in "features.json" to find is it old version (v12)
                var json = JObject.Parse(featStr);
                if (json["_"]?["V"] != null && (int)json["_"]["V"] == 1) // detect old "features.json" format (v12)
                {
                    // get stored features from old format
                    Features.Stored = GetFeaturesFromOldFormat(json);

                    // rename old file format "features.json" to "features.json.v12.bak"
                    var oldFeatureFilePathForBackup = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson + ".v12.bak");
                    if (File.Exists(oldFeatureFilePathForBackup)) File.Delete(oldFeatureFilePathForBackup);
                    File.Move(featureFilePath, oldFeatureFilePathForBackup);

                    // save "features.json" in new format
                    if (!SaveFeatures(Features.Stored)) return wrapLog("can't save features", false);
                }
                else // get stored features in new format
                    Features.Stored = JsonConvert.DeserializeObject<FeatureListStored>(featStr);

                return wrapLog("ok, features loaded", true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("load feature failed:" + e.Message, false);
            }
        }

        /// <summary>
        /// Load features from json old format (v12)
        /// </summary>
        /// <param name="json"></param>
        private FeatureListStored GetFeaturesFromOldFormat(JObject json)
        {
            var features = new FeatureListStored();

            var fs = (string) json["Entity"]["Attributes"]["Custom"]["Features"]["*"];
            var oldFeatures = JObject.Parse(fs);
            
            features.Fingerprint = (string) oldFeatures["fingerprint"];
            
            foreach (var f in (JArray) oldFeatures["features"])
            {
                features.Features.Add(new FeatureConfig()
                {
                    Id = (Guid) f["id"],
                    Enabled = (bool) f["enabled"],
                    Expires = (DateTime) f["expires"],
                });
            }

            return features;
        }

        /// <summary>
        /// Save "features.json"
        /// </summary>
        [PrivateApi]
        public bool SaveFeatures(FeatureListStored features)
        {
            var wrapLog = Log.Call<bool>();

            // save new format (v13)
            var json = JsonConvert.SerializeObject(features,
                //JsonSettings.Defaults()
                // reduce datetime serialization precision from 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'
                new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });

            var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);
            
            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);

            var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

            try
            {
                File.WriteAllText(featureFilePath, json);
                return wrapLog("ok, features saved ", true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("save features failed:" + e.Message, false);
            }
        }
    }
}
