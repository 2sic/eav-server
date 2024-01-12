using System;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public class SysFeatureDetectorNetFramework() : SysFeatureDetector(DefStatic, true)
{

    public static SysFeature DefStatic { get; } = new(
        "NetFramework",
        new("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
        "Dot Net Framework"
    );

#if NETFRAMEWORK
#else
    public SysFeatureDetectorNetFramework() : base(DefStatic, false) { }
#endif

}