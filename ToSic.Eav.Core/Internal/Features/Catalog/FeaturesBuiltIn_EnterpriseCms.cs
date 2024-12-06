using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForEnterpriseCms = BuildRule(BuiltInLicenses.EnterpriseCms, true);
    internal static List<FeatureLicenseRule> ForEnterpriseCmsDisabled = BuildRule(BuiltInLicenses.EnterpriseCms, false);

    // WIP / Beta in v13
    public static readonly Feature SharedApps = new() {
        NameId = "SharedApps",
        Guid = new("bb6656ef-fb81-4943-bf88-297e516d2616"),
        Name = "Share Apps to Reuse on Many Sites",
        IsPublic = false,
        Ui = false,
        Description = "Allows you to define shared global Apps which can be inherited and re-used on many Sites.",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForEnterpriseCms
    };

    public static readonly Feature PermissionsByLanguage = new()
    {
        NameId = "PermissionsByLanguage",
        Guid = new("fc1efaaa-89a0-446d-83de-89e20b3ce0d7"),
        Name = "Edit-Permissions by Language",
        IsPublic = false,
        Ui = false,
        Description = "Configure who can edit what language in the Edit UI.",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForEnterpriseCms
    };

    public static readonly Feature EditUiDisableDraft = new() {
        NameId = "EditUiDisableDraft",
        Guid = new("09cc2d62-e640-49dc-a267-2312aff97f55"),
        Name = "Edit-UI - Disable draft mode",
        IsPublic = false,
        Ui = false,
        Description = "Completely disable draft-mode in the Edit UI.",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForEnterpriseCmsDisabled
    };
}