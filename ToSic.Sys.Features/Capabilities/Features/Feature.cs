using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Performance;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Capabilities.Features;

[PrivateApi("no good reason to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Feature: Aspect
{
    #region Static Constructors
    
    private static IReadOnlyList<FeatureLicenseRule> CreateLicenseRules(IEnumerable<FeatureLicenseRule>? licRules, string nameId, Guid guid)
    {
        var newRules = licRules
                           ?.ToListOpt()
                       ?? [];
        // Create virtual license rule, so it can be enabled by its own GUID
        var ownLicenseDefinition = new FeatureSet.FeatureSet
        {
            NameId = LicenseConstants.LicenseCustom,
            Priority = 0,
            Name = $"Feature = {nameId}",
            Guid = guid,
            Description = $"Feature {nameId} ({guid})"
        };
        var ownRule = new FeatureLicenseRule(ownLicenseDefinition, true);
        newRules.Add(ownRule);
        return newRules.ToImmutableSafe();
    }


    /// <summary>
    /// Constructor for unknown feature - which only has a GUID to identify it
    /// </summary>
    /// <param name="unknownFeatureGuid"></param>
    public static Feature UnknownFeature(Guid unknownFeatureGuid) =>
        new()
        {
            NameId = unknownFeatureGuid.ToString(),
            Guid = unknownFeatureGuid,
            Name = "Unknown Feature",
            IsPublic = false,
            Ui = false,
            Description = "Unknown feature",
            Security = FeatureSecurity.Unknown,
            LicenseRules = null!
        };

    #endregion

    /// <summary>
    /// If true, this feature will be provided to the Ui
    /// If null or false, it won't be given to the Ui
    /// </summary>
    /// <remarks>
    /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
    /// </remarks>
    public bool Ui { get; init; }

    /// <summary>
    /// If true, this feature will be provided to the Ui
    /// If null or false, it won't be given to the Ui
    /// </summary>
    /// <remarks>
    /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
    /// </remarks>
    public bool IsPublic { get; init; }


    /// <summary>
    /// If true, this feature has security implications
    /// If null or false, it's either unknown or doesn't have security implications
    /// </summary>
    public FeatureSecurity Security { get; init; } = FeatureSecurity.Unknown;

    /// <summary>
    /// Determine if this feature can be activated by the user.
    /// Note: naming is not ideal, since it can be confused with the new configuration of v20
    /// </summary>
    public virtual bool IsConfigurable => true;

#pragma warning disable CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.

    /// <summary>
    /// The link which will be used to show more details online.
    /// eg: https://patrons.2sxc.org/rf?ContentSecurityPolicy
    /// </summary>
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    public virtual string Link
    {
        get => field ??= $"{PatronsUrl}/rf?{NameId}";
        init;
    }

    public required IEnumerable<FeatureLicenseRule> LicenseRules { get; init; }

    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    public IReadOnlyList<FeatureLicenseRule> LicenseRulesList => field ??= CreateLicenseRules(LicenseRules, NameId, Guid);

    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    public Requirement Requirement => field ??= new(FeatureConstants.RequirementFeature, NameId);

#pragma warning restore CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.

    public bool EnableForSystemTypes { get; init; }

    public FeatureDisabledBehavior DisabledBehavior { get; init; } = FeatureDisabledBehavior.Disable;

    /// <summary>
    /// New v18.05... WIP
    /// </summary>
    public bool ScopedToModule { get; init; }

    public string? ConfigurationContentType { get; init; }
}