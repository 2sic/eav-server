using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Configuration
{
    public class Features
    {
        public const bool AllowUnsignedFeatures = true; // testing mode!
        public const string TypeName = "FeaturesConfiguration";
        public const string FeaturesField = "Features";
        public const string SignatureField = "Signature";

        public const string FeaturesValidationSignature2Sxc930 =
                "MIIDxjCCAq6gAwIBAgIQOlgW44m37ohELal3ruEqjTANBgkqhkiG9w0BAQsFADBnMQ4wDAYDVQQHDAVCdWNoczERMA8GA1UECAwI" +
                "U3RHYWxsZW4xCzAJBgNVBAYTAkNIMRQwEgYDVQQLDAtEZXZlbG9wbWVudDENMAsGA1UECgwEMnNpYzEQMA4GA1UEAwwHMnN4YyBD" +
                "QTAeFw0xODA0MjQwNjU2NTlaFw0yMDEyMzEyMjAwMDBaMGcxDjAMBgNVBAcMBUJ1Y2hzMREwDwYDVQQIDAhTdEdhbGxlbjELMAkG" +
                "A1UEBhMCQ0gxFDASBgNVBAsMC0RldmVsb3BtZW50MQ0wCwYDVQQKDAQyc2ljMRAwDgYDVQQDDAcyc3hjIENBMIIBIjANBgkqhkiG" +
                "9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3aWG/UZBzD6CYsECiUK0h4c7An5vYMpktIDm1iSFFpJWUUDCZsdWUf8+3F9tb7Ur6JQtKYRu" +
                "WjccfV9Cb7meDinlV8IjJBRR8yPem/xmMj2Wxkpg9KcL19p108DsgMjm9+rLza+dqEHSPMFeS4c06PsXTWXvj46M8tm60Sq1cxSh" +
                "dqjnc4Byl4S2FE1/I57CsR3ZYSkJTiKM2C9drmEN070ptlvevGIuj7TPg8TE7otiLuwxb0G1mE9YXfJ5xniCnY8vy/K02yJakj7g" +
                "BumrgrlCLEUQ7Un3QoPSA9m/95ZHSMFLKH5h03bNS18pCGmEtYIMUITX/zotvjujKQNP8wIDAQABo24wbDAOBgNVHQ8BAf8EBAMC" +
                "AgQwEwYDVR0lBAwwCgYIKwYBBQUHAwMwEgYDVR0RBAswCYIHMnN4YyBDQTASBgNVHRMBAf8ECDAGAQH/AgEAMB0GA1UdDgQWBBRW" +
                "HUCWksFvOcx5Up2rUl/hNLnjlDANBgkqhkiG9w0BAQsFAAOCAQEAS6TYKzRJ/FLPEgeOnGg9D4FA7KHKsiV2jeF7q+YMO0+57mVb" +
                "XEwATNp7nYZFvKrHs/wxm+sULyt4BUrQQJ5Wwd+qRzdCzR85mebyhpaQx9+ffvvrD89ueDat4YlBk+jaAQHzMfjcARZ2xXr4CRDA" +
                "cURfVQ064UicolDoAed3JfZ1XbIpYpUPK0uDDwOmsnNkwVJb1fm1z+MKTRNORnZDZCPfwVlXu32xwG1/YzJqDNnqOd0zY8H4Mj/x" +
                "V+pokOjj/fBOjNiSfpI+7KkolNM43ZhLSw8TYStDZuf0WsSrU4vF0ROMyiynNhyebpPX21d/MB0PEfZ82uNXBrXTrBPFog==";

        public static bool Valid { get; private set; }

        internal static FeatureList Stored => _stored ?? (_stored = Load());
        private static FeatureList _stored; 

        public static IEnumerable<Feature> All => (_merged ?? (_merged = Merge(Stored, Catalog))).Features;
        private static FeatureList _merged;

        public static IEnumerable<Feature> Ui => All
            .Where(f => f.Enabled && f.Ui == true);

        public static bool Enabled(Guid id) => All.Any(f => f.Id == id && f.Enabled);

        public static bool Enabled(IEnumerable<Guid> ids) => ids.All(Enabled);

        public static bool EnabledOrException(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
        {
            var enabled = Enabled(features);
            exception = enabled ? null : new FeaturesDisabledException(message, features);
            return enabled;
        }

        public static string InfoLink 
            => _infoLink ?? (_infoLink = Factory.Resolve<ISystemConfiguration>().FeaturesHelpLink);
        private static string _infoLink;

        public static string MsgMissingSome(Guid id) 
            => $"Feature {id} not enabled - see {InfoLink}";
        public static string MsgMissingSome(IEnumerable<Guid> ids) 
            => $"Features {string.Join(", ", ids.Where(i => !Enabled(i)).Select(i => i.ToString()))} not enabled - see {InfoLink}";

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        public static void Reset()
        {
            _merged = null;
            _stored = null;
            Global.Reset(); // important, otherwise this is cached too
        }

        private static FeatureList Load()
        {
            FeatureListWithFingerprint feats = null;
            try
            {
                var entity = Global.For(TypeName);
                var featStr = entity?.GetBestValue<string>(FeaturesField);
                var signature = entity?.GetBestValue<string>(SignatureField);

                // Verify signature from security-system
                if (!string.IsNullOrWhiteSpace(featStr))
                {
                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        try
                        {
                            var data = new UnicodeEncoding().GetBytes(featStr);
                            Valid = Sha256.VerifyBase64(FeaturesValidationSignature2Sxc930, signature, data);
                        }
                        catch { /* ignore */ }
                    }

                    if (Valid || AllowUnsignedFeatures)
                    {
                        FeatureListWithFingerprint feats2 = null;
                        if (featStr.StartsWith("{"))
                            feats2 = JsonConvert.DeserializeObject<FeatureListWithFingerprint>(featStr);

                        if (feats2 != null)
                        {
                            var fingerprint = feats2.Fingerprint;
                            if (fingerprint != Fingerprint.System)
                                Valid = false;

                            if (Valid || AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            CacheTimestamp = DateTime.Now.Ticks;
            return feats ?? new FeatureList();
        }

        private static FeatureList Merge(FeatureList config, FeatureList cat)
        {
            var feats = config.Features.Select(f =>
            {
                var inCat = cat.Features.FirstOrDefault(c => c.Id == f.Id);
                return new Feature
                {
                    Id = f.Id,
                    Enabled = f.Enabled,
                    Expires = f.Expires,
                    Public = f.Public ?? inCat?.Public,
                    Ui = f.Ui ?? inCat?.Ui
                };
            }).ToList();

            return new FeatureList(feats);

        }

        public static long CacheTimestamp { get; private set; }

        public static bool CacheChanged(long compareTo) => compareTo != CacheTimestamp;




        /// <summary>
        /// The catalog contains known features, and knows if they are used in the UI
        /// This is important, because the installation specific list often won't know about
        /// Ui or not. 
        /// </summary>
        /// <remarks>
        /// this is a temporary solution, because most features are from 2sxc (not eav)
        /// so later on this must be injected or something
        /// </remarks>
        public static FeatureList Catalog = new FeatureList(new List<Feature>
        {
            // released features
            new Feature(FeatureIds.PublicForms, true, false),
            new Feature(FeatureIds.PublicUpload, true, false),
            new Feature(FeatureIds.UseAdamInWebApi, false, false),

            new Feature(FeatureIds.PermissionCheckUserId, true, false),
            new Feature(FeatureIds.PermissionCheckGroups, true, false),

            // Beta features
            new Feature(FeatureIds.PasteImageClipboard, true, true),
            new Feature(FeatureIds.Angular5Ui, false,false)

        });


    }
}

