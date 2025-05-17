using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.SysData;

public static class FeaturesToRawEntity
{
    public static IRawEntity ToRawEntity(this FeatureState state)
        => new RawEntity
        {
            Guid = state.Feature.Guid,
            Values = new Dictionary<string, object>
            {
                { nameof(state.NameId), state.NameId },
                { Attributes.TitleNiceName, state.Feature.Name },
                { nameof(Aspect.Description), state.Feature.Description },
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
                { nameof(state.Feature.Link), state.Feature.Link },
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

    public static IRawEntity ToRawEntity(this FeatureSetState state)
        => new RawEntity
        {
            Guid = state.Feature.Guid,
            Values = new Dictionary<string, object>
            {
                // Properties describing the License
                // { Attributes.NameIdNiceName, License.Name },
                { Attributes.TitleNiceName, state.Feature.Name },
                { nameof(state.Feature.NameId), state.Feature.NameId },
                { nameof(state.LicenseKey), state.LicenseKey },
                { nameof(state.Feature.Description), state.Feature.Description },
                { nameof(state.Feature.AutoEnable), state.Feature.AutoEnable },
                { nameof(state.Feature.Priority), state.Feature.Priority },
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