using ToSic.Eav.SysData;
// ReSharper disable ConvertToPrimaryConstructor

namespace ToSic.Eav.Internal.Features;

public class SysFeatureDetectorNetCore : SysFeatureDetector
{

    private static readonly SysFeature DefStatic = new()
    {
        NameId = "NetCore",
        Guid = new("57c306d5-ec3f-47e2-ad3a-ae871eb96a41"),
        Name = "Net Core",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };

#if NETFRAMEWORK
    public SysFeatureDetectorNetCore() : base(DefStatic, false) { }

#else
        public SysFeatureDetectorNetCore() : base(DefStatic, true) { }
#endif

}