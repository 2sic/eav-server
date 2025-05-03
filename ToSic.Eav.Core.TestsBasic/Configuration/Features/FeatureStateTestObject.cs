using ToSic.Eav.SysData;

namespace ToSic.Eav.Configuration.Features;

/// <summary>
/// Test object to reduce count on constructors of FeatureState
/// </summary>
public class FeatureStateTestObject(
    Feature aspect,
    DateTime expiration,
    bool enabled,
    string msgShort,
    string msgLong,
    bool allowedByLicense,
    bool enabledByDefault,
    bool? enabledInConfiguration,
    Dictionary<string, object> configuration) : FeatureState(
    aspect,
    expiration,
    enabled,
    msgShort,
    msgLong,
    allowedByLicense,
    enabledByDefault,
    enabledInConfiguration,
    configuration);