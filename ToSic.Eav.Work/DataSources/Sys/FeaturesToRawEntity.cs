using ToSic.Eav.Data.Raw;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.FeatureSet;

namespace ToSic.Eav.SysData;

public static class FeaturesToRawEntity
{
    public static IRawEntity ToRawEntity2(this FeatureState state)
        => new RawEntity
        {
            Guid = state.Aspect.Guid,
            Values = new Dictionary<string, object>
            {
                { nameof(state.NameId), state.NameId },
                { Attributes.TitleNiceName, state.Aspect.Name },
                { nameof(Aspect.Description), state.Aspect.Description },
                { nameof(state.IsEnabled), state.IsEnabled },
                { nameof(state.EnabledByDefault), state.EnabledByDefault },
                // Not important, don't include
                //{ "EnabledReason", EnabledReason },
                //{ "EnabledReasonDetailed", EnabledReasonDetailed },
                //{ "SecurityImpact", Security?.Impact },
                //{ "SecurityMessage", Security?.Message },
                { nameof(state.EnabledInConfiguration), state.EnabledInConfiguration },
                { nameof(state.Expiration), state.Expiration },
                { nameof(state.IsForEditUi), state.IsForEditUi },
                { $"{nameof(state.License)}{nameof(state.License.Name)}", state.License?.Name ?? Constants.NullNameId },
                { $"{nameof(state.License)}{nameof(state.License.Guid)}", state.License?.Guid ?? Guid.Empty },
                { nameof(state.AllowedByLicense), state.AllowedByLicense },
                { nameof(state.Aspect.Link), state.Aspect.Link },
                { nameof(state.IsPublic), state.IsPublic },
            }
        };

    /// <summary>
    /// Important: We are creating an object which is basically the License.
    /// So even though we're creating an Entity from the LicenseState,
    /// this is just because it knows more about the License than the
    /// root definition does.
    /// But basically it should be the License + State information.
    /// </summary>

    public static IRawEntity ToRawEntity2(this FeatureSetState state)
        => new RawEntity
        {
            Guid = state.Aspect.Guid,
            Values = new Dictionary<string, object>
            {
                // Properties describing the License
                // { Attributes.NameIdNiceName, License.Name },
                { Attributes.TitleNiceName, state.Aspect.Name },
                { nameof(state.Aspect.NameId), state.Aspect.NameId },
                { nameof(state.LicenseKey), state.LicenseKey },
                { nameof(state.Aspect.Description), state.Aspect.Description },
                { nameof(state.Aspect.AutoEnable), state.Aspect.AutoEnable },
                { nameof(state.Aspect.Priority), state.Aspect.Priority },
                // The License Condition is an internal property
                // Used when checking conditions on other objects - if this license is what is expected
                //{ "LicenseConditionType", License.Condition.Type },
                //{ "LicenseConditionNameId", License.Condition.NameId },
                //{ "LicenseConditionIsEnabled", License.Condition.IsEnabled },

                // Properties describing the state/enabled
                { nameof(state.IsEnabled), state.IsEnabled },
                { nameof(state.EnabledInConfiguration), state.EnabledInConfiguration },
                { nameof(state.Valid), state.Valid },
                { nameof(state.Expiration), state.Expiration },
                { nameof(state.ExpirationIsValid), state.ExpirationIsValid },
                { nameof(state.SignatureIsValid), state.SignatureIsValid },
                { nameof(state.FingerprintIsValid), state.FingerprintIsValid },
                { nameof(state.VersionIsValid), state.VersionIsValid },
                { nameof(state.Owner), state.Owner }
            },
        };

}