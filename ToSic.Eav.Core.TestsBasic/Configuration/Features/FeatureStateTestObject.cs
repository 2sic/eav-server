using ToSic.Eav.SysData;

namespace ToSic.Eav.Configuration.Features;

/// <summary>
/// Test object to reduce count on constructors of FeatureState
/// </summary>
public record FeatureStateTestObject(
    Feature Aspect,
    DateTime Expiration,
    bool Enabled,
    string EnabledReason,
    string EnabledReasonDetailed,
    bool AllowedByLicense,
    bool EnabledByDefault,
    bool? EnabledInConfiguration,
    Dictionary<string, object>? Configuration) : FeatureState(
    Aspect,
    Expiration,
    Enabled,
    EnabledReason,
    EnabledReasonDetailed,
    AllowedByLicense,
    EnabledByDefault,
    EnabledInConfiguration,
    Configuration);