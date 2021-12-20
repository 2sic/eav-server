using System;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Caching;
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
            _runtime = runtime.Init(Log);
        }

        private readonly IFingerprint _fingerprint;
        private readonly IRuntime _runtime;
        private readonly IAppsCache _appsCache;
        public readonly IFeaturesInternal Features;

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

            // Now do a normal reload of configuration and features
            Reload();
        }

        /// <summary>
        /// 2021-11-16 2dm - experimental, working on moving global/preset data into a normal AppState #PresetInAppState
        /// </summary>
        private void LoadPresetApp()
        {
            var wrapLog = Log.Call();
            Log.Add("Try to load global app-state");
            var appState = _runtime.AppState();
            _appsCache.Add(appState);
            wrapLog("ok");
        }

        private bool _startupAlreadyRan;

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void Reload()
        {
            FeatureListWithFingerprint feats = null;
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
                    {
                        try
                        {
                            var data = new UnicodeEncoding().GetBytes(featStr);
                            FeaturesService.ValidInternal = new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930, signature, data);
                        }
                        catch { /* ignore */ }
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
                    {
                        FeatureListWithFingerprint feats2 = null;
                        if (featStr.StartsWith("{"))
                            feats2 = JsonConvert.DeserializeObject<FeatureListWithFingerprint>(featStr);

                        if (feats2 != null)
                        {
                            var fingerprint = feats2.Fingerprint;
                            if (fingerprint != _fingerprint.GetSystemFingerprint()) 
                                FeaturesService.ValidInternal = false;

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            Features.Stored = feats ?? new FeatureList();
            Features.CacheTimestamp = DateTime.Now.Ticks;
        }
    }
}
