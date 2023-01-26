﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Configuration.RequirementCheckFeature;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureDefinition: IHasIdentityNameId
    {
        #region Constructors

        public FeatureDefinition(string nameId, Guid guid, string name, bool isPublic, bool ui, string description, FeatureSecurity security,
            IEnumerable<FeatureLicenseRule> licRules)
        {
            Guid = guid;
            NameId = nameId;
            Name = name;
            Security = security;
            Public = isPublic;
            Ui = ui;
            Description = description;
            Condition = new Condition(ConditionIsFeature, nameId);
            LicenseRules = CreateLicenseRules(licRules);    // must run at the end, as properties are needed
        }

        private IReadOnlyList<FeatureLicenseRule> CreateLicenseRules(IEnumerable<FeatureLicenseRule> licRules)
        {
            var newRules = licRules?.ToList() ?? new List<FeatureLicenseRule>(0);
            // Create virtual license rule so it can be enabled by its own GUID
            var ownLicenseDefinition = new LicenseDefinition(0, $"Feature: {NameId}", Guid, $"Feature {NameId} ({Guid})");
            var ownRule = new FeatureLicenseRule(ownLicenseDefinition, true);
            newRules.Add(ownRule);
            return newRules.AsReadOnly();
        }


        /// <summary>
        /// Constructor for unknown feature - which only has a GUID to identify it
        /// </summary>
        /// <param name="unknownFeatureGuid"></param>
        internal FeatureDefinition(Guid unknownFeatureGuid)
        {
            Guid = unknownFeatureGuid;
            Condition = new Condition(ConditionIsFeature, Guid.ToString());
        }

        #endregion

        /// <summary>
        /// Feature GUID
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Feature String ID
        /// </summary>
        public string NameId { get; }

        /// <summary>
        /// A nice name / title for showing in UIs
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A nice description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        public bool Ui { get; }

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        public bool Public { get; }


        /// <summary>
        /// If true, this feature has security implications
        /// If null or false, it's either unknown or doesn't have security implications
        /// </summary>
        public FeatureSecurity Security { get; }

        internal IReadOnlyList<FeatureLicenseRule> LicenseRules { get; }

        public Condition Condition { get; }
    }
}
