using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronInfrastructureAutoEnabled = BuildRule(BuiltInLicenses.PatronInfrastructure, true);


    public static readonly Feature SqlCompressDataTimeline = new(
        nameof(SqlCompressDataTimeline),
        new("87325de8-d671-4731-bd58-186ff6de6329"),
        "Shrink your DB size by up to 80%. Enables compressed JSON for the change-history which is rarely accessed.",
        false,
        true,
        "todo",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronInfrastructureAutoEnabled
    );

    public static readonly Feature SqlLoadPerformance = new(
        nameof(SqlLoadPerformance),
        new("f63d8b5f-21aa-4fbd-8e14-5507ff6426a2"),
        "Improve SQL performance accessing the data base, for example when loading data. Can double the speed of loading data.",
        false,
        true,
        "todo",
        FeaturesCatalogRules.Security0Neutral,
        ForPatronInfrastructureAutoEnabled
    );

}