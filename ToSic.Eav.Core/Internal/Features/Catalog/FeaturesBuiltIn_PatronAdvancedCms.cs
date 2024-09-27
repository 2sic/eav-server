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

}