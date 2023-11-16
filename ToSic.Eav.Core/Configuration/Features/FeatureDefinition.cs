using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Configuration.RequirementCheckFeature;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class FeatureDefinition: AspectDefinition
    {
        #region Constructors

        public FeatureDefinition(string nameId, Guid guid, string name, bool isPublic, bool ui, string description, FeatureSecurity security,
            IEnumerable<FeatureLicenseRule> licRules): base(nameId, guid, name, description)
        {
            Security = security;
            Public = isPublic;
            Ui = ui;
            Condition = new Condition(ConditionIsFeature, nameId);
            LicenseRules = CreateLicenseRules(licRules, nameId, guid);    // must run at the end, as properties are needed
        }
        static IReadOnlyList<FeatureLicenseRule> CreateLicenseRules(IEnumerable<FeatureLicenseRule> licRules, string nameId, Guid guid)
        {
            var newRules = licRules?.ToList() ?? new List<FeatureLicenseRule>(0);
            // Create virtual license rule so it can be enabled by its own GUID
            var ownLicenseDefinition = new LicenseDefinition(BuiltInLicenses.LicenseCustom, 0, $"Feature: {nameId}", guid, $"Feature {nameId} ({guid})");
            var ownRule = new FeatureLicenseRule(ownLicenseDefinition, true);
            newRules.Add(ownRule);
            return newRules.AsReadOnly();
        }


        /// <summary>
        /// Constructor for unknown feature - which only has a GUID to identify it
        /// </summary>
        /// <param name="unknownFeatureGuid"></param>
        internal FeatureDefinition(Guid unknownFeatureGuid): this(unknownFeatureGuid.ToString(), unknownFeatureGuid, "Unknown Feature", false, false, "Unknown feature", FeatureSecurity.Unknown, null)
        {
        }

        #endregion

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


        public virtual bool IsConfigurable => true;

        /// <summary>
        /// The link which will be used to show more details online.
        /// eg: https://patrons.2sxc.org/rf?ContentSecurityPolicy
        /// </summary>
        public virtual string Link => $"{PatronsUrl}/rf?{NameId}";

        internal IReadOnlyList<FeatureLicenseRule> LicenseRules { get; }

        public Condition Condition { get; }
    }
}
