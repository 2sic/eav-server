using ToSic.Sys.Capabilities.Features;

// ReSharper disable ConvertToPrimaryConstructor

namespace ToSic.Sys.Capabilities.SysFeatures;

public class SysFeatureDetectorNetCore : SysFeatureDetector
{

    /// <summary>
    /// The feature definition.
    /// </summary>
    /// <remarks>
    /// Internal for unit testing
    /// </remarks>
    internal static readonly SysFeature DefStatic = new()
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