/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */
using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Process;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseState: IHasRawEntity<IRawEntity>
    {
        public LicenseState() { }

        public string Title { get; internal set; }
        public string LicenseKey { get; internal set; }

        public Guid EntityGuid { get; internal set; }

        public LicenseDefinition License { get; internal set; }

        public bool Enabled => EnabledInConfiguration && Valid;

        /// <summary>
        /// The state as toggled in the settings - ATM always true, as we don't read the settings
        /// </summary>
        public bool EnabledInConfiguration { get; internal set; } = true;

        public bool Valid => ExpirationIsValid && SignatureIsValid && FingerprintIsValid && VersionIsValid;

        public DateTime Expiration { get; internal set; }

        public bool ExpirationIsValid { get; internal set; }

        public bool SignatureIsValid { get; internal set; }

        public bool FingerprintIsValid { get; internal set; }

        public bool VersionIsValid { get; internal set; }
        
        public string Owner { get; internal set; }

        #region IHasNewEntity
        
        /// <summary>
        /// Important: We are creating an object which is basically the License.
        /// So even though we're creating an Entity from the LicenseState,
        /// this is just because it knows more about the License than the
        /// root definition does.
        /// But basically it should be the License + State information.
        /// </summary>
        public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
        {
            Guid = License.Guid,
            Values = new Dictionary<string, object>
            {
                // Properties describing the License
                // { Attributes.NameIdNiceName, License.Name },
                { Attributes.TitleNiceName, License.Name },
                { nameof(License.NameId), License.NameId },
                { nameof(LicenseKey), LicenseKey },
                { nameof(License.Description), License.Description },
                { nameof(License.AutoEnable), License.AutoEnable },
                { nameof(License.Priority), License.Priority },
                // The License Condition is an internal property
                // Used when checking conditions on other objects - if this license is what is expected
                //{ "LicenseConditionType", License.Condition.Type },
                //{ "LicenseConditionNameId", License.Condition.NameId },
                //{ "LicenseConditionIsEnabled", License.Condition.IsEnabled },

                // Properties describing the state/enabled
                { nameof(Enabled), Enabled },
                { nameof(EnabledInConfiguration), EnabledInConfiguration },
                { nameof(Valid), Valid },
                { nameof(Expiration), Expiration },
                { nameof(ExpirationIsValid), ExpirationIsValid },
                { nameof(SignatureIsValid), SignatureIsValid },
                { nameof(FingerprintIsValid), FingerprintIsValid },
                { nameof(VersionIsValid), VersionIsValid },
                { nameof(Owner), Owner }
            },
        });
        private readonly GetOnce<IRawEntity> _newEntity = new GetOnce<IRawEntity>();

        #endregion


    }
}
