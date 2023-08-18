using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Serialization;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class EavSystemLoader : LoaderBase
    {
        private readonly IRuntime _appStateLoader;
        private readonly AppsCacheSwitch _appsCache;
        public readonly IFeaturesInternal Features;
        private readonly FeatureConfigManager _featureConfigManager;
        private readonly ILogStore _logStore;
        private readonly IAppStates _appStates;
        private readonly LicenseLoader _licenseLoader;
        private readonly SystemFingerprint _fingerprint; // note: must be of type SystemFingerprint, not IFingerprint

        #region Constructor / DI

        public EavSystemLoader(
            SystemFingerprint fingerprint,  // note: must be of type SystemFingerprint, not IFingerprint
            IRuntime runtime,
            AppsCacheSwitch appsCache, 
            IFeaturesInternal features, 
            FeatureConfigManager featureConfigManager, 
            LicenseLoader licenseLoader,
            ILogStore logStore,
            IAppStates appStates
        ) : base(logStore, $"{EavLogs.Eav}SysLdr")
        {
            Log.A("System Load");
            ConnectServices(
                _fingerprint = fingerprint,
                _appsCache = appsCache,
                _logStore = logStore,
                _appStates = appStates,
                _appStateLoader = runtime,
                Features = features,
                _featureConfigManager = featureConfigManager,
                _licenseLoader = licenseLoader
            );
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
            var assemblyLoadLog = new Log(EavLogs.Eav + "AssLdr", null, "Load Assemblies");
            _logStore.Add(LogNames.LogStoreStartUp, assemblyLoadLog);

            var ldo = Log.Fn(timer: true);
            AssemblyHandling.GetTypes(assemblyLoadLog);

            // Build the cache of all system-types. Must happen before everything else
            ldo.A("Try to load global app-state");
            var presetApp = _appStateLoader.LoadFullAppState();
            _appsCache.Value.Add(presetApp);

            LoadLicenseAndFeatures();
            ldo.Done("ok");
        }

        /// <summary>
        /// Standalone Features loading - to make the features API available in tests
        /// </summary>
        public void LoadLicenseAndFeatures()
        {
            var l = Log.Fn();
            try
            {
                var presetApp = _appStates.GetPresetApp();
                l.A($"presetApp:{presetApp != null}");

                var licEntities = presetApp.List
                    .OfType(LicenseEntity.TypeNameId)
                    .Select(e => new LicenseEntity(e))
                    .ToList();
                l.A($"licEnt:{licEntities?.Count}");

                // Check all licenses and show extra message
                var enterpriseLicenses = licEntities
                    .Select(lic =>
                    {
                        var entFp = lic.AsEnterprise();
                        l.A(entFp.ValidityMessage);
                        return entFp;
                    })
                    .ToList();
                l.A($"entLic:{enterpriseLicenses?.Count}");

                _licenseLoader.Init(enterpriseLicenses).LoadLicenses();
            }
            catch (Exception e)
            {
                l.Ex(e);
                l.Done("error");
                return;
            }

            // Now do a normal reload of configuration and features
            ReloadFeatures();
            l.Done("ok");
        }
        private bool _startupAlreadyRan;

        /// <summary>
        /// Reset the features stored by loading from 'features.json'.
        /// </summary>
        [PrivateApi]
        public bool ReloadFeatures() => SetFeaturesStored(LoadFeaturesStored());


        private bool SetFeaturesStored(FeatureListStored stored = null) 
            => Features.UpdateFeatureList(stored ?? new FeatureListStored());


        /// <summary>
        /// Load features stored from 'features.json'.
        /// When old format is detected, it is converted to new format.
        /// </summary>
        /// <returns></returns>
        private FeatureListStored LoadFeaturesStored()
        {
            var l = Log.Fn<FeatureListStored>();
            try
            {
                var (filePath, fileContent) = _featureConfigManager.LoadFeaturesFile();
                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileContent)) 
                    return l.ReturnNull("ok, but 'features.json' is missing");

                // handle old 'features.json' format
                var stored = _featureConfigManager.ConvertOldFeaturesFile(filePath, fileContent);
                if (stored != null) 
                    return l.ReturnAndLog(stored, "converted to new features.json");

                // return features stored
                return l.ReturnAndLog(JsonSerializer.Deserialize<FeatureListStored>(fileContent, JsonOptions.UnsafeJsonWithoutEncodingHtml), "ok, features loaded");
            }
            catch (Exception e)
            {
                l.Ex(e);
                return l.ReturnNull("load feature failed:" + e.Message);
            }
        }


        /// <summary>
        /// Update existing features config in "features.json". 
        /// </summary>
        [PrivateApi]
        public bool UpdateFeatures(List<FeatureManagementChange> changes)
        {
            var l = Log.Fn<bool>($"c:{changes?.Count ?? -1}");
            var saved = _featureConfigManager.SaveFeaturesUpdate(changes);
            SetFeaturesStored(FeatureListStoredBuilder(changes));
            return l.ReturnAndLog(saved, "ok, updated");
        }

        private FeatureListStored FeatureListStoredBuilder(List<FeatureManagementChange> changes)
        {
            var updatedIds = changes.Select(f => f.FeatureGuid);

            var storedFeaturesButNotUpdated = Features.All
                .Where(f => f.EnabledInConfiguration.HasValue && !updatedIds.Contains(f.Guid))
                .Select(FeatureConfigManager.FeatureConfigBuilder).ToList();
            
            var updatedFeatures = changes
                .Where(f => f.Enabled.HasValue)
                .Select(FeatureConfigManager.FeatureConfigBuilder).ToList();

            return new FeatureListStored
            {
                Features = storedFeaturesButNotUpdated.Union(updatedFeatures).ToList(),
                Fingerprint = _fingerprint.GetFingerprint()
            };
        }
    }
}
