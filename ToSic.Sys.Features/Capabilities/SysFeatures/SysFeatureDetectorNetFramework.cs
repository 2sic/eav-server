using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Detects if the current runtime is .NET Framework.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class SysFeatureDetectorNetFramework() : SysFeatureDetector(DefStatic,
#if NETFRAMEWORK
    true
#else
    false
#endif
)
{
    /// <summary> The feature definition. </summary>
    /// <remarks> Internal for unit testing </remarks>
    internal static SysFeature DefStatic { get; } = new()
    {
        NameId = "NetFramework",
        Guid = new("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
        Name = "Dot Net Framework",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };
}
