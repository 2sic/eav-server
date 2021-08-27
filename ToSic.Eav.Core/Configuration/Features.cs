using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// The Features lets your code find out what features are currently enabled/disabled in the environment.
    /// It's important to detect if the admin must activate certain features to let your code do it's work.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public partial class Features
    {

        #region Constants / Signatures

        internal const bool AllowUnsignedFeatures = true; // testing mode!
        internal const string TypeName = "FeaturesConfiguration";
        internal const string FeaturesField = "Features";
        internal const string SignatureField = "Signature";
        public const string FeaturesJson = "features.json";

        public const string FeaturesPath = Constants.FolderDataCustom + "/configurations/";

        [PrivateApi("no good reason to publish this")]
        internal const string FeaturesValidationSignature2Sxc930 =
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

        #endregion



        /// <summary>
        /// Informs you if the enabled features are valid or not - meaning if they have been countersigned by the 2sxc features system.
        /// As of now, it's not enforced, but in future it will be. 
        /// </summary>
        /// <returns>true if the features were signed correctly</returns>
        public static bool Valid { get; internal set; }

        [PrivateApi]
        internal static FeatureList Stored
        {
            get => _stored;
            set
            {
                _stored = value;
                _all = null;
            }
        }

        private static FeatureList _stored;

        [PrivateApi]
        public static IEnumerable<Feature> All => (_all ?? (_all = Merge(Stored, Catalog))).Features;
        private static FeatureList _all;

        [PrivateApi]
        public static IEnumerable<Feature> Ui => All.Where(f => f.Enabled && f.Ui == true);

        /// <summary>
        /// Checks if a feature is enabled
        /// </summary>
        /// <param name="guid">The feature Guid</param>
        /// <returns>true if the feature is enabled</returns>
        public static bool Enabled(Guid guid) => All.Any(f => f.Id == guid && f.Enabled);

        /// <summary>
        /// Checks if a list of features are enabled, in case you need many features to be activated.
        /// </summary>
        /// <param name="guids">list/array of Guids</param>
        /// <returns>true if all features are enabled, false if any one of them is not</returns>
        public static bool Enabled(IEnumerable<Guid> guids) => guids.All(Enabled);

        [PrivateApi]
        public bool EnabledOrException(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var enabled = Enabled(features);
            exception = enabled ? null : new FeaturesDisabledException(message + " - " + MsgMissingSome(features), features);
            // ReSharper restore PossibleMultipleEnumeration
            return enabled;
        }



        [PrivateApi]
        public string MsgMissingSome(IEnumerable<Guid> ids) 
            => $"Features {string.Join(", ", ids.Where(i => !Enabled(i)).Select(id => $"{InfoLinkRoot}{id}"))} not enabled - see also {HelpLink}";



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

        /// <summary>
        /// Just for debugging
        /// </summary>
        [PrivateApi]
        internal static long CacheTimestamp { get; set; }

    }
}

