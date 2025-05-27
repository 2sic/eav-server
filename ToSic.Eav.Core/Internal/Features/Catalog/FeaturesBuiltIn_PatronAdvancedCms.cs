using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronAdvancedCms, true);

    public static readonly Feature CopyrightManagement = new()
    {
        NameId = nameof(CopyrightManagement),
        Guid = new("2114297a-d1e7-40d2-88d7-e44cd1111bfa"),
        Name = "Copyright Management for Content (WIP/Beta v17)",
        IsPublic = false,
        Ui = true,
        Description = "If enabled, Copyright Management will appear in image toolbars and in future do more. ",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronAdvancedCmsAutoEnabled
    };

    public static readonly Feature SharedAppCode = new()
    {
        NameId = nameof(SharedAppCode),
        Guid = new("30efe5c2-b4db-4228-93de-bcf7ec8e16bd"),
        Name = "Share AppCode from a shared app in multiple sites.",
        IsPublic = false,
        Ui = true,
        Description = "If enabled, you can use AppCode in a shared location for similar Apps in multiple sites.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronAdvancedCmsAutoEnabled
    };

}