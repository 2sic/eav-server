﻿using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using static ToSic.Eav.Internal.Features.FeatureRequirementCheck;

namespace ToSic.Eav.SysData;

[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Feature(
    string nameId,
    Guid guid,
    string name,
    bool isPublic,
    bool ui,
    string description,
    FeatureSecurity security,
    IEnumerable<FeatureLicenseRule> licRules,
    bool enableForSystemTypes = false,
    FeatureDisabledBehavior disabledBehavior = FeatureDisabledBehavior.Disable
): Aspect(nameId, guid, name, description)
{
    #region Constructors

    private static IReadOnlyList<FeatureLicenseRule> CreateLicenseRules(IEnumerable<FeatureLicenseRule> licRules, string nameId, Guid guid)
    {
        var newRules = licRules?.ToList() ?? [];
        // Create virtual license rule, so it can be enabled by its own GUID
        var ownLicenseDefinition = new FeatureSet(BuiltInLicenses.LicenseCustom, 0, $"Feature: {nameId}", guid, $"Feature {nameId} ({guid})");
        var ownRule = new FeatureLicenseRule(ownLicenseDefinition, true);
        newRules.Add(ownRule);
        return newRules.AsReadOnly();
    }


    /// <summary>
    /// Constructor for unknown feature - which only has a GUID to identify it
    /// </summary>
    /// <param name="unknownFeatureGuid"></param>
    internal Feature(Guid unknownFeatureGuid): this(unknownFeatureGuid.ToString(), unknownFeatureGuid, "Unknown Feature", false, false, "Unknown feature", FeatureSecurity.Unknown, null)
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
    public bool Ui { get; } = ui;

    /// <summary>
    /// If true, this feature will be provided to the Ui
    /// If null or false, it won't be given to the Ui
    /// </summary>
    /// <remarks>
    /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
    /// </remarks>
    public bool Public { get; } = isPublic;


    /// <summary>
    /// If true, this feature has security implications
    /// If null or false, it's either unknown or doesn't have security implications
    /// </summary>
    public FeatureSecurity Security { get; } = security;


    public virtual bool IsConfigurable => true;

    /// <summary>
    /// The link which will be used to show more details online.
    /// eg: https://patrons.2sxc.org/rf?ContentSecurityPolicy
    /// </summary>
    public virtual string Link => $"{PatronsUrl}/rf?{NameId}";

    internal IReadOnlyList<FeatureLicenseRule> LicenseRules { get; } = CreateLicenseRules(licRules, nameId, guid); // must run at the end, as properties are needed

    public Requirement Requirement { get; } = new(ConditionIsFeature, nameId);

    public bool EnableForSystemTypes { get; } = enableForSystemTypes;

    public FeatureDisabledBehavior DisabledBehavior { get; } = disabledBehavior;
}