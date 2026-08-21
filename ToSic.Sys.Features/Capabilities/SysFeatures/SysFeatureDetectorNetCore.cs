using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Detects if the current runtime is .NET Core.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
internal class SysFeatureDetectorNetCore() : SysFeatureDetector(DefStatic,
#if NETFRAMEWORK
    false
#else
    true
#endif
)
{
    /// <summary> The feature definition. </summary>
    /// <remarks> Internal for unit testing </remarks>
    internal static readonly SysFeature DefStatic = new()
    {
        NameId = "NetCore",
        Guid = new("57c306d5-ec3f-47e2-ad3a-ae871eb96a41"),
        Name = "Net Core",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };
}
