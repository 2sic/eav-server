using System;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Information about an enabled feature
    /// Note that this is also used as a DTO for the edit-UI, so don't just rename fields or anything.
    /// </summary>
    [PrivateApi("no good reason to publish this")]
    public class FeatureState
    {
        private readonly FeatureDefinition _featureDefinition;

        public Guid Guid => _featureDefinition.Guid;
        public string NameId => _featureDefinition.NameId;
        public string Name => _featureDefinition.Name;
        public string Description => _featureDefinition.Description;

        public string License => _featureDefinition?.LicenseRules.FirstOrDefault()?.LicenseDefinition.Name;


        /// <summary>
        /// Feature is enabled and hasn't expired yet
        /// </summary>
        /// <remarks>by default all features are disabled</remarks>
        public bool Enabled => _enabled && Expires > DateTime.Now;
        private readonly bool _enabled;


        /// <summary>
        /// Reason why it was enabled
        /// </summary>
        public string EnabledReason { get; }

        /// <summary>
        /// More detailed reason
        /// </summary>
        public string EnabledReasonDetailed { get; }

        /// <summary>
        /// Expiry of this feature
        /// </summary>
        public DateTime Expires { get; }


        public bool Ui => _featureDefinition.Ui;
        public bool Public => _featureDefinition.Public;
        public FeatureSecurity Security => _featureDefinition.Security;

        /// <summary>
        /// Indicate if this feature is allowed to be activated
        /// </summary>
        public bool LicenseEnabled { get; }

        /// <summary>
        /// The stored enabled state.
        /// The EnabledStored would be null, true or false.
        /// Null if it was not stored. 
        /// </summary>
        public bool? EnabledStored { get; }

        /// <summary>
        /// If this feature is enabled by default (assuming the license requirements are met)
        /// </summary>
        public bool EnabledByDefault { get; }



        public FeatureState(FeatureDefinition definition, DateTime expires, bool enabled, string msgShort, string msgLong, bool licenseEnabled, bool enabledByDefault, bool? enabledStored)
        {
            _featureDefinition = definition;
            Expires = expires;
            _enabled = enabled;
            EnabledReason = msgShort;
            EnabledReasonDetailed = msgLong;
            LicenseEnabled = licenseEnabled;
            EnabledStored = enabledStored;
            EnabledByDefault = enabledByDefault;
        }
    }
}
