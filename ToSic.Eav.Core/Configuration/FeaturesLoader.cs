using System;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FeaturesLoader
    {
        #region Constructor / DI

        public FeaturesLoader(IFingerprint fingerprint)
        {
            Fingerprint = fingerprint;
        }
        public IFingerprint Fingerprint { get; }

        #endregion

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void Reload()
        {
            // Reset global data which stores the features
            Global.Reset();
            LoadNew();
        }


        private void LoadNew()
        {
            FeatureListWithFingerprint feats = null;
            try
            {
                var entity = Global.For(Features.TypeName);
                var featStr = entity?.Value<string>(Features.FeaturesField);
                var signature = entity?.Value<string>(Features.SignatureField);

                // Verify signature from security-system
                if (!string.IsNullOrWhiteSpace(featStr))
                {
                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        try
                        {
                            var data = new UnicodeEncoding().GetBytes(featStr);
                            Features.Valid = Sha256.VerifyBase64(Features.FeaturesValidationSignature2Sxc930, signature, data);
                        }
                        catch { /* ignore */ }
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (Features.Valid || Features.AllowUnsignedFeatures)
                    {
                        FeatureListWithFingerprint feats2 = null;
                        if (featStr.StartsWith("{"))
                            feats2 = JsonConvert.DeserializeObject<FeatureListWithFingerprint>(featStr);

                        if (feats2 != null)
                        {
                            var fingerprint = feats2.Fingerprint;
                            if (fingerprint != Fingerprint.GetSystemFingerprint()) // Fingerprint.System)
                                Features.Valid = false;

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (Features.Valid || Features.AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            Features.CacheTimestamp = DateTime.Now.Ticks;
            Features.Stored = feats ?? new FeatureList();
            // return new Tuple<FeatureList, long>(feats ?? new FeatureList(), DateTime.Now.Ticks);
        }


    }
}
