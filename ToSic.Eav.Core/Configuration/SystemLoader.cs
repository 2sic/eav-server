using System;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;

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
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            _fingerprint = fingerprint;
            _appStateLoader = runtime.Init(Log);
        }

        private readonly IFingerprint _fingerprint;
        private readonly IRuntime _appStateLoader;
        private readonly IAppsCache _appsCache;
        public readonly IFeaturesInternal Features;

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

            // Build the cache of all system-types. Must happen before everything else
            LoadPresetApp();

            // V13 - Load Licenses
            // Avoid using DI, as otherwise someone could inject a different license loader
            new LicenseLoader(_appsCache, _fingerprint, Log).LoadLicenses();
            

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
            FeatureListStored feats = null;
            try
            {
                var presetApp = _appsCache.Get(null, Constants.PresetIdentity);

                var entity = presetApp.List.FirstOrDefaultOfType(FeatureConstants.TypeName);
                var featStr = entity?.Value<string>(FeatureConstants.FeaturesField);
                var signature = entity?.Value<string>(FeatureConstants.SignatureField);

                // Verify signature from security-system
                if (!string.IsNullOrWhiteSpace(featStr))
                {
                    if (!string.IsNullOrWhiteSpace(signature))
                        try
                        {
                            var data = new UnicodeEncoding().GetBytes(featStr);
                            FeaturesService.ValidInternal =
                                new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
                                    signature, data);
                        }
                        catch (Exception ex)
                        {
                            // Just log, and ignore
                            Log.Exception(ex);
                        }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
                    {
                        var feats2 = featStr.StartsWith("{") 
                            ? JsonConvert.DeserializeObject<FeatureListStored>(featStr) 
                            : null;

                        if (feats2 != null)
                        {
                            if (feats2.Fingerprint != _fingerprint.GetSystemFingerprint())
                                FeaturesService.ValidInternal = false;

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Just log and ignore
                Log.Exception(ex);
            }

            Features.Stored = feats ?? new FeatureListStored();
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
