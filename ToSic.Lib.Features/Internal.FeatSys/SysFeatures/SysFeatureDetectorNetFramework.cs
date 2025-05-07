using ToSic.Eav.SysData;
// ReSharper disable ConvertToPrimaryConstructor

namespace ToSic.Eav.Internal.Features;

public class SysFeatureDetectorNetFramework : SysFeatureDetector
{

    public static SysFeature DefStatic { get; } = new()
    {
        NameId = "NetFramework",
        Guid = new("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
        Name = "Dot Net Framework",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };
#if NETFRAMEWORK
    public SysFeatureDetectorNetFramework() : base(DefStatic, true) { }
#else
    public SysFeatureDetectorNetFramework() : base(DefStatic, false) { }
#endif

}