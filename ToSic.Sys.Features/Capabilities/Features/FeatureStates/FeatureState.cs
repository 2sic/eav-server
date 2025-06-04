using ToSic.Lib.Data;
using ToSic.Lib.Helpers;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Capabilities.SysFeatures;

namespace ToSic.Sys.Capabilities.Features;

/// <summary>
/// Information about an enabled feature
/// </summary>
[PrivateApi("no good reason to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record FeatureState(
    Feature Feature,
    DateTime Expiration,
    bool Enabled,
    string EnabledReason,
    string EnabledReasonDetailed,
    bool AllowedByLicense,
    bool EnabledByDefault,
    bool? EnabledInConfiguration,
    Dictionary<string, object>? Configuration)
    : AspectState<Feature>(Feature, Enabled), IHasIdentityNameId
{
    public static FeatureState SysFeatureState(SysFeature definition, bool enabled)
        => new(definition, 
            LicenseConstants.UnlimitedExpiry,
            enabled,
            "System Feature",
            "System Feature, managed by the system; can't be changed interactively.",
            true,
            true,
            null,
            null);

    public string NameId => Aspect.NameId;

    public FeatureSet.FeatureSet? License => _license.Get(() => Aspect.LicenseRulesList?.FirstOrDefault()?.FeatureSet);
    private readonly GetOnce<FeatureSet.FeatureSet?> _license = new();

    /// <summary>
    /// Feature is enabled and hasn't expired yet.
    /// Will test the date every time it's used.
    /// </summary>
    /// <remarks>by default all features are disabled</remarks>
    public override bool IsEnabled => base.IsEnabled && Expiration > DateTime.Now;

    /// <summary>
    /// Reason why it was enabled
    /// </summary>
    public string EnabledReason { get; } = EnabledReason;

    /// <summary>
    /// More detailed reason
    /// </summary>
    public string EnabledReasonDetailed { get; } = EnabledReasonDetailed;

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    public DateTime Expiration { get; } = Expiration;

    /// <summary>
    /// Determines if this feature should be available in the normal EditUI.
    /// This only applies to normal users.
    /// Admins and Super-Users will always get all the features in the Edit-UI, to allow for better UI hints. 
    /// </summary>
    public bool IsForEditUi => Aspect.Ui;

    /// <summary>
    /// Determines if non-admins should still know about this feature in the UI
    /// </summary>
    public bool IsPublic => Aspect.IsPublic;

    public FeatureSecurity Security => Aspect.Security;

    /// <summary>
    /// Indicate if this feature is allowed to be activated
    /// </summary>
    public bool AllowedByLicense { get; } = AllowedByLicense;

    /// <summary>
    /// The stored enabled state.
    /// The EnabledStored would be null, true or false.
    /// Null if it was not stored. 
    /// </summary>
    public bool? EnabledInConfiguration { get; } = EnabledInConfiguration;

    /// <summary>
    /// If this feature is enabled by default (assuming the license requirements are met)
    /// </summary>
    public bool EnabledByDefault { get; } = EnabledByDefault;

    public Dictionary<string, object>? Configuration { get; }= Configuration;

}