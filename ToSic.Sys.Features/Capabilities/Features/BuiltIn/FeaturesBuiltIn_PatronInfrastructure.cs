using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronInfrastructureAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronInfrastructure, true);


    public static readonly Feature SqlCompressDataTimeline = new()
    {
        NameId = nameof(SqlCompressDataTimeline),
        Guid = new("87325de8-d671-4731-bd58-186ff6de6329"),
        Name = "Shrink your DB size by up to 80%",
        IsPublic = false,
        Ui = true,
        Description = "Shrink your DB size by up to 80%. Enables compressed JSON for the change-history which is rarely accessed.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled
    };

}