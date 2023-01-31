using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("hide implementation")]
    public class FeaturesService: IFeaturesInternal
    {
        public FeaturesService(FeaturesCatalog featuresCatalog) => _featuresCatalog = featuresCatalog;
        private readonly FeaturesCatalog _featuresCatalog;

        public IEnumerable<FeatureState> All => _all ?? (_all = Merge(Stored, _featuresCatalog.List));
        private static List<FeatureState> _all;

        /// <summary>
        /// List of all enabled features with their guids and nameIds
        /// </summary>
        public HashSet<string> EnabledFeatures => _enabledFeatures.Get(() => new HashSet<string>(All
                .Where(f => f.Enabled)
                .SelectMany(f => new[] { f.NameId, f.Guid.ToString() })
                .Distinct(InvariantCultureIgnoreCase),
                InvariantCultureIgnoreCase)
            );
        private readonly GetOnce<HashSet<string>> _enabledFeatures = new GetOnce<HashSet<string>>();

        public IEnumerable<FeatureState> UiFeaturesForEditors => All.Where(f => f.Enabled && f.ForEditUi);

        public bool Enabled(Guid guid) => All.Any(f => f.Guid == guid && f.Enabled);
        
        public bool Enabled(IEnumerable<Guid> guids) => guids.All(Enabled);

        public bool IsEnabled(params string[] nameIds)
        {
            if (nameIds == null || nameIds.Length == 0) return true;
            return nameIds.All(name => EnabledFeatures.Contains(name?.Trim()));
        }

        public bool IsEnabled(params FeatureDefinition[] features) 
            => IsEnabled(features?.Select(f => f.NameId).ToArray());

        public bool Valid => ValidInternal;
        public static bool ValidInternal;
        
        public bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var enabled = Enabled(features);
            exception = enabled ? null : new FeaturesDisabledException(message + " - " + MsgMissingSome(features.ToArray()));
            // ReSharper restore PossibleMultipleEnumeration
            return enabled;
        }

        [PrivateApi]
        public string MsgMissingSome(params Guid[] ids)
        {
            var missing = ids
                .Where(i => !Enabled(i))
                .Select(id =>
                {
                    var feat = All.FirstOrDefault(f => f.Guid == id);
                    return new { Id = id, feat?.NameId };
                });

            var messages = missing.Select(f => $"'{f.NameId}'");

            return
                $"Features {string.Join(", ", messages)} not enabled - see also https://r.2sxc.org/features";
        }


        #region Static Caches

        [PrivateApi]
        public FeatureListStored Stored
        {
            get => _stored;
            set
            {
                _stored = value;
                _all = null;
                _enabledFeatures.Reset();
                CacheTimestamp = DateTime.Now.Ticks;
            }
        }
        private static FeatureListStored _stored;


        private static List<FeatureState> Merge(FeatureListStored config, IReadOnlyCollection<FeatureDefinition> featuresCat)
        {
            var licService = new LicenseService();

            var allFeats = featuresCat.Select(f =>
            {
                var enabled = false;
                var licenseEnabled = false;
                var msgShort = "default";
                var message = " by default";
                var expiry = DateTime.MinValue;

                // Check if the required license is active
                var enabledRule = f.LicenseRules.FirstOrDefault(lr => licService.IsEnabled(lr.LicenseDefinition));
                if (enabledRule != null)
                {
                    licService.Enabled.TryGetValue(enabledRule.LicenseDefinition.Guid, out var licenseState);
                    var specialExpiry = licenseState?.Expiration;
                    enabled = enabledRule.EnableFeatureByDefault;
                    licenseEnabled = true; // The license is active, so it's allowed to enable this
                    msgShort = enabledRule.LicenseDefinition.Name;
                    message = $" by default with license {enabledRule.LicenseDefinition.Name}";
                    expiry = specialExpiry ?? BuiltInLicenses.UnlimitedExpiry;
                }

                // Check if the configuration would enable this feature
                var inConfig = config?.Features.FirstOrDefault(cf => cf.Id == f.Guid);
                if (inConfig != null)
                {
                    enabled = licenseEnabled && inConfig.Enabled;
                    expiry = inConfig.Expires;
                    msgShort = licenseEnabled ? "configuration" : "unlicensed";
                    message = licenseEnabled ? " by configuration" : " - requires license";
                }

                return new FeatureState(f, expiry, enabled, msgShort, (enabled ? "Enabled" : "Disabled") + message,
                    licenseEnabled, enabledByDefault: enabledRule?.EnableFeatureByDefault ?? false, enabledStored: inConfig?.Enabled);
            }).ToList();

            // Find additional, un matching features which are not known in the catalog
            var missingFeatures = config?.Features
                .Where(f => featuresCat.All(fd => fd.Guid != f.Id))
                .Select(f => new FeatureState(new FeatureDefinition(f.Id), f.Expires, f.Enabled, "configuration", "Configured manually", 
                    licenseEnabled: false, enabledByDefault: false,  enabledStored: f.Enabled));

            var final = (missingFeatures == null ? allFeats : allFeats.Union(missingFeatures)).ToList();
            return final;
        }


        [PrivateApi]
        public long CacheTimestamp
        {
            get => _cacheTimestamp;
            set
            {
                _cacheTimestamp = value;
                FeaturesChanged?.Invoke(this, EventArgs.Empty); // publish event so lightspeed can flush cache
            }
        }
        private long _cacheTimestamp;

        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;

        // Custom event for LightSpeed
        [PrivateApi] 
        public event EventHandler FeaturesChanged;

        #endregion
    }
}
