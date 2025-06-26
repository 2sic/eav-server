
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

// TODO: probably move to the Sxc project or somewhere similar

public class SysFeatureDetectorRazor() : SysFeatureDetector(DefStatic, true)
{

    private static readonly SysFeature DefStatic = new()
    {
        NameId = "Razor",
        Guid = new("1301aa40-45e0-4349-8a23-2f05ed4120da"),
        Name = "Razor",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };
}