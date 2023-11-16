using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.StringComparer;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// WARNING: this is used as a singleton / static
    /// not quite sure any more why, either the name is hard-coded in some apps or we felt like it's a performance improvement
    /// </summary>
    [PrivateApi("hide implementation")]
    public class FeaturesService: IFeaturesInternal
    {
        #region Constructor

        /// <summary>
        /// warning: singleton - don't use any complex services/dependencies here
        /// </summary>
        /// <param name="featuresCatalog"></param>
        public FeaturesService(FeaturesCatalog featuresCatalog)
        {
            _featuresCatalog = featuresCatalog;
        }

        private readonly FeaturesCatalog _featuresCatalog;

        #endregion


        public IEnumerable<FeatureState> All => AllStaticCache.Get(() => Merge(Stored, _featuresCatalog.List, _staticSysFeatures));
        private static readonly GetOnce<List<FeatureState>> AllStaticCache = new GetOnce<List<FeatureState>>();

        /// <summary>
        /// List of all enabled features with their guids and nameIds
        /// </summary>
        internal HashSet<string> EnabledFeatures => _enabledFeatures ?? (_enabledFeatures  = new HashSet<string>(All
                .Where(f => f.IsEnabled)
                .SelectMany(f => new[] { f.NameId, f.Definition.Guid.ToString() })
                .Distinct(InvariantCultureIgnoreCase),
            InvariantCultureIgnoreCase));
        private HashSet<string> _enabledFeatures; // had to step back from GetOnce, because of "Error Unable to marshal host object to interpreter space"

        public IEnumerable<FeatureState> UiFeaturesForEditors => All.Where(f => f.IsEnabled && f.IsForEditUi);

        public bool IsEnabled(Guid guid) => All.Any(f => f.Definition.Guid == guid && f.IsEnabled);
        
        public bool IsEnabled(IEnumerable<Guid> guids) => guids.All(IsEnabled);

        public bool IsEnabled(params string[] nameIds)
        {
            if (nameIds == null || nameIds.Length == 0) return true;
            return nameIds.All(name => EnabledFeatures.Contains(name?.Trim()));
        }

        public FeatureState Get(string nameId) => All.FirstOrDefault(f => f.Definition.Name == nameId || f.NameId == nameId);

        public bool IsEnabled(params FeatureDefinition[] features) 
            => IsEnabled(features?.Select(f => f.NameId).ToArray());

        public bool Valid => ValidInternal;
        public static bool ValidInternal; // ATM always false; is used by a static class - not sure why this even exists as I don't think it's set anywhere
        
        public bool IsEnabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var enabled = IsEnabled(features);
            exception = enabled ? null : new FeaturesDisabledException(message + " - " + MsgMissingSome(features.ToArray()));
            // ReSharper restore PossibleMultipleEnumeration
            return enabled;
        }

        [PrivateApi]
        public string MsgMissingSome(params Guid[] ids)
        {
            var missing = ids
                .Where(i => !IsEnabled(i))
                .Select(id =>
                {
                    var feat = All.FirstOrDefault(f => f.Definition.Guid == id);
                    return new { Id = id, feat?.NameId };
                });

            var messages = missing.Select(f => $"'{f.NameId}'");

            return
                $"Features {string.Join(", ", messages)} not enabled - see also https://go.2sxc.org/features";
        }


        #region Static Caches

        [PrivateApi]
        public FeatureListStored Stored => _staticStored;

        private static FeatureListStored _staticStored;
        private static List<FeatureState> _staticSysFeatures;

        public bool UpdateFeatureList(FeatureListStored newList, List<FeatureState> sysFeatures)
        {
            _staticStored = newList;
            _staticSysFeatures = sysFeatures;
            AllStaticCache.Reset();
            _enabledFeatures = null;
            CacheTimestamp = DateTime.Now.Ticks;
            FeaturesChanged?.Invoke(this, EventArgs.Empty); // publish event so lightspeed can flush cache
            return true;
        }


        private static List<FeatureState> Merge(FeatureListStored config, IReadOnlyCollection<FeatureDefinition> featuresCat, List<FeatureState> sysFeatureStates)
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
                    if (expiry == DateTime.MinValue) expiry = inConfig.Expires; // set expiry by configuration (when is not set by license)
                    msgShort = licenseEnabled ? "configuration" : "unlicensed";
                    message = licenseEnabled ? " by configuration" : " - requires license";
                }

                return new FeatureState(f, expiry, enabled, msgShort, (enabled ? "Enabled" : "Disabled") + message,
                    licenseEnabled, enabledByDefault: enabledRule?.EnableFeatureByDefault ?? false, enabledInConfiguration: inConfig?.Enabled);
            }).ToList();

            // Find additional, un matching features which are not known in the catalog
            var missingFeatures = config?.Features
                .Where(f => featuresCat.All(fd => fd.Guid != f.Id))
                .Select(f => new FeatureState(new FeatureDefinition(f.Id), f.Expires, f.Enabled, "configuration", "Configured manually", 
                    allowedByLicense: false, enabledByDefault: false,  enabledInConfiguration: f.Enabled));

            var final = (missingFeatures == null ? allFeats : allFeats.Union(missingFeatures)).ToList();
            if (sysFeatureStates.SafeAny()) final = final.Union(sysFeatureStates).ToList();
            return final;
        }


        [PrivateApi]
        public long CacheTimestamp { get; private set; }

        public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp != dependentTimeStamp;

        // Custom event for LightSpeed
        [PrivateApi] 
        public event EventHandler FeaturesChanged;

        #endregion
    }
}
