using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("hide implementation")]
    public class FeaturesService: IFeaturesInternal
    {
        public IEnumerable<FeatureState> All => (_all ?? (_all = Merge(Stored, FeaturesCatalog.Initial)));
        private static List<FeatureState> _all;

        /// <summary>
        /// List of all enabled features with their guids and nameIds
        /// </summary>
        public HashSet<string> EnabledFeatures => _enabledFeatures ?? (_enabledFeatures = new HashSet<string>(All
                .Where(f => f.Enabled)
                .SelectMany(f => new string[] { f.NameId, f.Guid.ToString() })
                .Distinct(StringComparer.InvariantCultureIgnoreCase),
                StringComparer.InvariantCultureIgnoreCase)
            );
        private HashSet<string> _enabledFeatures;

        public IEnumerable<FeatureState> EnabledUi => All.Where(f => f.Enabled && f.Ui);

        public bool Enabled(Guid guid) => All.Any(f => f.Guid == guid && f.Enabled);
        
        public bool Enabled(IEnumerable<Guid> guids) => guids.All(Enabled);

        public bool IsEnabled(params string[] nameIds)
        {
            if (nameIds == null || nameIds.Length == 0) return true;
            return nameIds.All(name => EnabledFeatures.Contains(name?.Trim()));
        }

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
        public string HelpLink => "https://2sxc.org/help?tag=features";

        /// <inheritdoc />
        public string InfoLinkRoot => "https://2sxc.org/r/f/";

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
            var licService = new LicenseService();

            var allFeats = cat.Select(f =>
            {
                var enabled = false;
                var allowed = false;
                var msgShort = "default";
                var message = " by default";
                var expiry = DateTime.MinValue;

                // Check if the required license is active
                var enabledRule = f.LicenseRules.FirstOrDefault(lr => licService.IsEnabled(lr.LicenseDefinition));
                if (enabledRule != null)
                {
                    licService.Enabled.TryGetValue(enabledRule.LicenseDefinition, out var licenseState);
                    var specialExpiry = licenseState?.Expiration;
                    enabled = enabledRule.EnableFeatureByDefault || enabledRule.LicenseDefinition.AutoEnable;
                    allowed = true; // The license is active, so it's allowed to enable this
                    msgShort = enabledRule.LicenseDefinition.Name;
                    message = $" by default with license {enabledRule.LicenseDefinition.Name}";
                    expiry = specialExpiry ?? LicenseCatalog.UnlimitedExpiry;
                }

                // Check if the configuration would enable this feature
                var inConfig = config.Features.FirstOrDefault(cf => cf.Id == f.Guid);
                if (inConfig != null)
                {
                    enabled = allowed && inConfig.Enabled;
                    expiry = inConfig.Expires;
                    msgShort = allowed ? "configuration" : "unlicensed";
                    message = allowed ? " by configuration" : " - requires license";
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
