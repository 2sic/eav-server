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
    internal class FeaturesLoader: HasLog
    {
        /// <summary>
        /// Constructor - not meant for DI
        /// </summary>
        internal FeaturesLoader(IAppsCache appsCache, IFingerprint fingerprint, LogHistory logHistory, ILog parentLog) : base(LogNames.Eav + "LicLdr", parentLog, "Load Features")
        {
            _appsCache = appsCache;
            _fingerprint = fingerprint;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
        }
        private readonly IAppsCache _appsCache;
        private readonly IFingerprint _fingerprint;


        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        public FeatureListStored LoadFeatures()
        {
            var wrapLog = Log.Call<FeatureListStored>();
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

            return wrapLog("ok", feats ?? new FeatureListStored());
        }
    }
}
