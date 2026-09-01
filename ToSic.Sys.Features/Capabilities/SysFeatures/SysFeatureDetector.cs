using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Abstract base class for trivial system feature detectors which determine if a feature is enabled or not.
/// It allows for quickly creating feature detectors which already have all the specs necessary (to reduce code).
/// </summary>
/// <param name="definition">The feature definition.</param>
/// <param name="isEnabled">Indicates whether the feature is enabled.</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public abstract class SysFeatureDetector(SysFeature definition, bool isEnabled = default) : ISysFeatureDetector
{
    /// <summary>
    /// Flag if enabled. INTERNAL for unit testing.
    /// </summary>
    public bool IsEnabled { get; } = isEnabled;

    public FeatureState GetState() => new(
        definition,
        LicenseConstants.UnlimitedExpiry,
        IsEnabled,  
        "System Feature",
        "System Feature, managed by the system; can't be changed interactively.",
        true,
        true,
        null,
        null
    );
}
