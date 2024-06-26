using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Internal.Licenses;
using ToSic.Lib.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.SysData;

/// <summary>
/// Information about an enabled feature
/// </summary>
[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureState(
    Feature aspect,
    DateTime expiration,
    bool enabled,
    string msgShort,
    string msgLong,
    bool allowedByLicense,
    bool enabledByDefault,
    bool? enabledInConfiguration)
    : AspectState<Feature>(aspect, enabled), IHasRawEntity<IRawEntity>, IHasIdentityNameId
{
    public static FeatureState SysFeatureState(SysFeature definition, bool enabled)
        => new(definition, BuiltInLicenses.UnlimitedExpiry, enabled,
            "System Feature", "System Feature, managed by the system; can't be changed interactively.", true, true,
            null);

    public string NameId => Aspect.NameId;

    public FeatureSet License => _license.Get(() => Aspect.LicenseRules?.FirstOrDefault()?.FeatureSet);
    private readonly GetOnce<FeatureSet> _license = new();

    /// <summary>
    /// Feature is enabled and hasn't expired yet.
    /// Will test the date every time it's used.
    /// </summary>
    /// <remarks>by default all features are disabled</remarks>
    public override bool IsEnabled => base.IsEnabled && Expiration > DateTime.Now;

    /// <summary>
    /// Reason why it was enabled
    /// </summary>
    public string EnabledReason { get; } = msgShort;

    /// <summary>
    /// More detailed reason
    /// </summary>
    public string EnabledReasonDetailed { get; } = msgLong;

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    public DateTime Expiration { get; } = expiration;

    /// <summary>
    /// Determines if this feature should be available in the normal EditUI.
    /// This only applies to normal users.
    /// Admins and Super-Users will always get all the features in the Edit-UI, to allow for better UI hints. 
    /// </summary>
    public bool IsForEditUi => Aspect.Ui;

    /// <summary>
    /// Determines if non-admins should still know about this feature in the UI
    /// </summary>
    public bool IsPublic => Aspect.Public;

    public FeatureSecurity Security => Aspect.Security;

    /// <summary>
    /// Indicate if this feature is allowed to be activated
    /// </summary>
    public bool AllowedByLicense { get; } = allowedByLicense;

    /// <summary>
    /// The stored enabled state.
    /// The EnabledStored would be null, true or false.
    /// Null if it was not stored. 
    /// </summary>
    public bool? EnabledInConfiguration { get; } = enabledInConfiguration;

    /// <summary>
    /// If this feature is enabled by default (assuming the license requirements are met)
    /// </summary>
    public bool EnabledByDefault { get; } = enabledByDefault;

    #region IHasNewEntity

    [JsonIgnore]
    public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
    {
        Guid = Aspect.Guid,
        Values = new Dictionary<string, object>
        {
            { nameof(NameId), NameId },
            { Attributes.TitleNiceName, Aspect.Name },
            { nameof(Aspect.Description), Aspect.Description },
            { nameof(IsEnabled), IsEnabled },
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
            { nameof(Aspect.Link), Aspect.Link },
            { nameof(IsPublic), IsPublic },
        }
    });
    private readonly GetOnce<IRawEntity> _newEntity = new();

    #endregion
}