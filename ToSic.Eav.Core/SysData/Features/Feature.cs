﻿using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using static ToSic.Eav.Internal.Features.FeatureRequirementCheck;

namespace ToSic.Eav.SysData;

[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record Feature: Aspect
{
    #region Constructors
    
    private static IReadOnlyList<FeatureLicenseRule> CreateLicenseRules(IEnumerable<FeatureLicenseRule> licRules, string nameId, Guid guid)
    {
        var newRules = licRules?.ToList() ?? [];
        // Create virtual license rule, so it can be enabled by its own GUID
        var ownLicenseDefinition = new FeatureSet
        {
            NameId = BuiltInLicenses.LicenseCustom,
            Priority = 0,
            Name = $"Feature = {nameId}",
            Guid = guid,
            Description = $"Feature {nameId} ({guid})"
        };
        var ownRule = new FeatureLicenseRule(ownLicenseDefinition, true);
        newRules.Add(ownRule);
        return newRules.AsReadOnly();
    }


    /// <summary>
    /// Constructor for unknown feature - which only has a GUID to identify it
    /// </summary>
    /// <param name="unknownFeatureGuid"></param>
    internal static Feature UnknownFeature(Guid unknownFeatureGuid) =>
        new()
        {
            NameId = unknownFeatureGuid.ToString(),
            Guid = unknownFeatureGuid,
            Name = "Unknown Feature",
            IsPublic = false,
            Ui = false,
            Description = "Unknown feature",
            Security = FeatureSecurity.Unknown,
            LicenseRules = null
        };

    #endregion

    /// <summary>
    /// If true, this feature will be provided to the Ui
    /// If null or false, it won't be given to the Ui
    /// </summary>
    /// <remarks>
    /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
    /// </remarks>
    public bool Ui { get; init; } = false;

    /// <summary>
    /// If true, this feature will be provided to the Ui
    /// If null or false, it won't be given to the Ui
    /// </summary>
    /// <remarks>
    /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
    /// </remarks>
    public bool IsPublic { get; init; } = false;


    /// <summary>
    /// If true, this feature has security implications
    /// If null or false, it's either unknown or doesn't have security implications
    /// </summary>
    public FeatureSecurity Security { get; init; } = FeatureSecurity.Unknown;


    public virtual bool IsConfigurable => true;

    /// <summary>
    /// The link which will be used to show more details online.
    /// eg: https://patrons.2sxc.org/rf?ContentSecurityPolicy
    /// </summary>
    public virtual string Link
    {
        get => field ??= $"{PatronsUrl}/rf?{NameId}";
        init => field = value;
    }

    public required IEnumerable<FeatureLicenseRule> LicenseRules { get; init; }

    internal IReadOnlyList<FeatureLicenseRule> LicenseRulesList => field ??= CreateLicenseRules(LicenseRules, NameId, Guid);

    public Requirement Requirement => field ??= new(ConditionIsFeature, NameId);

    public bool EnableForSystemTypes { get; init; }

    public FeatureDisabledBehavior DisabledBehavior { get; init; } = FeatureDisabledBehavior.Disable;

    /// <summary>
    /// New v18.05... WIP
    /// </summary>
    public bool ScopedToModule { get; init; }
}