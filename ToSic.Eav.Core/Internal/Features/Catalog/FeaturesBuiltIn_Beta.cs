using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForBeta = BuiltInLicenseRules.BuildRule(BuiltInLicenses.CoreBeta, false);
    public static List<FeatureLicenseRule> ForBetaEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.CoreBeta, true);

    // ATM only used in azing, so still easy to change

    public static readonly Feature AdamRestrictLookupToEntity = new()
    {
        NameId = nameof(AdamRestrictLookupToEntity),
        Guid = new("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
        Name = "ADAM - Restrict file lookup to the same entity (BETA, not final)",
        IsPublic = false,
        Ui = false,
        Description = "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForBeta
    };

}