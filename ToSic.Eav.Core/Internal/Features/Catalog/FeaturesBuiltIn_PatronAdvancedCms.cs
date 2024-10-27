using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronAdvancedCmsAutoEnabled = BuildRule(BuiltInLicenses.PatronAdvancedCms, true);

    public static readonly Feature CopyrightManagement = new(
        nameof(CopyrightManagement),
        new("2114297a-d1e7-40d2-88d7-e44cd1111bfa"),
        "Copyright Management for Content (WIP/Beta v17)",
        false,
        true,
        "If enabled, Copyright Management will appear in image toolbars and in future do more. ",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled
    );

    public static readonly Feature SharedAppCode = new(
        nameof(SharedAppCode),
        new("30efe5c2-b4db-4228-93de-bcf7ec8e16bd"),
        "Share AppCode from a shared app in multiple sites.",
        false,
        true,
        "If enabled, you can use AppCode in a shared location for similar Apps in multiple sites.",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronAdvancedCmsAutoEnabled
    );

}