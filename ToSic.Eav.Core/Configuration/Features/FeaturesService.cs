using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("hide implementation")]
    public class FeaturesService: IFeaturesInternal
    {
        public IEnumerable<FeatureState> All => (_all ?? (_all = Merge(Stored, FeaturesCatalog.Initial)));
        private static List<FeatureState> _all;

        public IEnumerable<FeatureState> Ui => All.Where(f => f.Enabled && f.Ui);

        public bool Enabled(Guid guid) => All.Any(f => f.Guid == guid && f.Enabled);
        
        public bool Enabled(IEnumerable<Guid> guids) => guids.All(Enabled);

        public bool Enabled(params string[] nameIds) => nameIds.All(name => All.Any(f => f.NameId == name && f.Enabled));

        public bool Valid => ValidInternal;
        public static bool ValidInternal;
        
        public bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
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

        #region Links

        /// <inheritdoc />
        public string HelpLink
        {
            get => _helpLink;
            set => _helpLink = value;
        }
        private static string _helpLink = "https://2sxc.org/help?tag=features";

        /// <inheritdoc />
        public string InfoLinkRoot
        {
            get => _infoLinkRoot;
            set => _infoLinkRoot = value;
        }
        private static string _infoLinkRoot = "https://2sxc.org/r/f/";

        #endregion

        #region Static Caches

        [PrivateApi]
        public FeatureListStored Stored
        {
            get => _stored;
            set
            {
                _stored = value;
                _all = null;
            }
        }
        private static FeatureListStored _stored;


        private static List<FeatureState> Merge(FeatureListStored config, IReadOnlyCollection<FeatureDefinition> cat)
        {
            var enabledLicenses = Licenses.Licenses.Enabled;

            var allFeats = cat.Select(f =>
            {
                var enabled = false;
                var msgShort = "default";
                var message = " by default";
                var expiry = DateTime.MinValue;
                var licenses = f.LicenseRules.FirstOrDefault(lr => Licenses.Licenses.IsEnabled(lr.LicenseType.Guid));
                if (licenses != null)
                {
                    enabled = licenses.DefaultEnabled;
                    msgShort = licenses.LicenseType.Name;
                    message = $" by default with license {licenses.LicenseType.Name}";
                    expiry = enabledLicenses.TryGetValue(licenses.LicenseType.Guid, out var lic) 
                        ? lic.Expiration 
                        : new DateTime(2099, 12, 31);
                }

                var inConfig = config.Features.FirstOrDefault(cf => cf.Id == f.Guid);
                if (inConfig != null)
                {
                    enabled = inConfig.Enabled;
                    expiry = inConfig.Expires;
                    msgShort = "configuration";
                    message = " by configuration";
                }
                return new FeatureState(f, expiry, enabled, msgShort, (enabled ? "Enabled" : "Disabled") + message);
            }).ToList();

            // Find additional, un matching features
            var missingFeatures = config.Features
                .Where(f => cat.All(fd => fd.Guid != f.Id))
                .Select(f => new FeatureState(new FeatureDefinition(f.Id), f.Expires, f.Enabled, "configuration", "Configured manually"));

            var final = allFeats.Union(missingFeatures).ToList();
            return final;
        }


        /// <summary>
        /// Just for debugging
        /// </summary>
        [PrivateApi]
        public long CacheTimestamp { get; set; }

        #endregion
    }
}
