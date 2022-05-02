using System;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static readonly FeatureDefinition WebFarmCache = new FeatureDefinition(
            "WebFarmCache",
            new Guid("11c0fedf-16a7-4596-900c-59e860b47965"),
            "Web Farm Cache",
            false,
            false,
            "Enables WebFarm Cache use in Dnn", FeaturesCatalogRules.Security0Improved,
            BuildRule(Licenses.BuiltInLicenses.WebFarmCache, true)
        );

    }
}
