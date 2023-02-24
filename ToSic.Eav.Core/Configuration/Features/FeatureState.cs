using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
using ToSic.Eav.Data.New;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Information about an enabled feature
    /// Note that this is also used as a DTO for the edit-UI, so don't just rename fields or anything.
    /// </summary>
    [PrivateApi("no good reason to publish this")]
    public class FeatureState: IHasNewEntity<INewEntity>, IHasIdentityNameId
    {
        /// <summary>
        /// Feature Definition can be null, if a feature was activated with an unknown ID
        /// </summary>
        private readonly FeatureDefinition _featureDefinition;

        public FeatureState(FeatureDefinition definition, DateTime expiration, bool enabled, string msgShort, string msgLong, bool allowedByLicense, bool enabledByDefault, bool? enabledInConfiguration)
        {
            _featureDefinition = definition;
            Expiration = expiration;
            _enabled = enabled;
            EnabledReason = msgShort;
            EnabledReasonDetailed = msgLong;
            AllowedByLicense = allowedByLicense;
            EnabledInConfiguration = enabledInConfiguration;
            EnabledByDefault = enabledByDefault;
        }


        public string NameId => _featureDefinition.NameId;
        public Guid Guid => _featureDefinition.Guid;

        public string Name => _featureDefinition.Name;
        public string Description => _featureDefinition.Description;

        public LicenseDefinition License => _license.Get(() => _featureDefinition.LicenseRules?.FirstOrDefault()?.LicenseDefinition);
        private readonly GetOnce<LicenseDefinition> _license = new GetOnce<LicenseDefinition>();

        /// <summary>
        /// Feature is enabled and hasn't expired yet
        /// </summary>
        /// <remarks>by default all features are disabled</remarks>
        public bool Enabled => _enabled && Expiration > DateTime.Now;
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
        public DateTime Expiration { get; }

        /// <summary>
        /// Determines if this feature should be available in the normal EditUI.
        /// This only applies to normal users.
        /// Admins and Super-Users will always get all the features in the Edit-UI, to allow for better UI hints. 
        /// </summary>
        public bool IsForEditUi => _featureDefinition.Ui;

        /// <summary>
        /// Determines if non-admins should still know about this feature in the UI
        /// </summary>
        public bool IsPublic => _featureDefinition.Public;
        public FeatureSecurity Security => _featureDefinition.Security;

        /// <summary>
        /// Indicate if this feature is allowed to be activated
        /// </summary>
        public bool AllowedByLicense { get; }

        /// <summary>
        /// The stored enabled state.
        /// The EnabledStored would be null, true or false.
        /// Null if it was not stored. 
        /// </summary>
        public bool? EnabledInConfiguration { get; }

        /// <summary>
        /// If this feature is enabled by default (assuming the license requirements are met)
        /// </summary>
        public bool EnabledByDefault { get; }

        /// <summary>
        /// The link which will be used to show more details online.
        /// eg: https://patrons.2sxc.org/rf?ContentSecurityPolicy
        /// </summary>
        public string Link => $"https://patrons.2sxc.org/rf?{NameId}";


        #region IHasNewEntity

        public INewEntity NewEntity => _newEntity.Get(() => new NewEntity
        {
            Guid = Guid,
            Values = new Dictionary<string, object>
            {
                { nameof(NameId), NameId },
                { Attributes.TitleNiceName, Name },
                { nameof(Description), Description },
                { nameof(Enabled), Enabled },
                { nameof(EnabledByDefault), EnabledByDefault },
                // Not important, don't include
                //{ "EnabledReason", EnabledReason },
                //{ "EnabledReasonDetailed", EnabledReasonDetailed },
                //{ "SecurityImpact", Security?.Impact },
                //{ "SecurityMessage", Security?.Message },
                { nameof(EnabledInConfiguration), EnabledInConfiguration },
                { nameof(Expiration), Expiration },
                { nameof(IsForEditUi), IsForEditUi },
                { $"{nameof(License)}{nameof(License.Name)}", License?.Name ?? Constants.NullNameId },
                { $"{nameof(License)}{nameof(License.Guid)}", License?.Guid ?? Guid.Empty },
                { nameof(AllowedByLicense), AllowedByLicense },
                { nameof(Link), Link },
                { nameof(IsPublic), IsPublic },
            }
        });
        private readonly GetOnce<INewEntity> _newEntity = new GetOnce<INewEntity>();

        #endregion
    }
}
