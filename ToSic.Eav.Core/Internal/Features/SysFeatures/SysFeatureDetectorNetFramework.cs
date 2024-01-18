using ToSic.Eav.SysData;
// ReSharper disable ConvertToPrimaryConstructor

namespace ToSic.Eav.Internal.Features;

public class SysFeatureDetectorNetFramework : SysFeatureDetector
{

    public static SysFeature DefStatic { get; } = new(
        "NetFramework",
        new("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
        "Dot Net Framework"
    );
#if NETFRAMEWORK
    public SysFeatureDetectorNetFramework() : base(DefStatic, true) { }
#else
    public SysFeatureDetectorNetFramework() : base(DefStatic, false) { }
#endif

}