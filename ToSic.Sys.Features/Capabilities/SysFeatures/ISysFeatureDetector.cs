using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Interface marking classes which can detect if a system feature (capability) is enabled or not.
/// All implementations of this interface will be queried once to provide feature feedback.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface ISysFeatureDetector
{
    /// <summary>
    /// Returns the current state of the feature as a FeatureState object.
    /// </summary>
    FeatureState GetState();
}
