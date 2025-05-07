#if DEBUG
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForTestingDisabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.CoreTesting, false);

    public static readonly Feature TestingFeature001 = new()
    {
        NameId = nameof(TestingFeature001),
        Guid = new("f6cad4f7-f7ad-4205-9887-f7e28443ea8f"),
        Name = "Just for testing!",
        IsPublic = false,
        Ui = false,
        Description = "Just for testing!",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForTestingDisabled
    };
}
#endif
