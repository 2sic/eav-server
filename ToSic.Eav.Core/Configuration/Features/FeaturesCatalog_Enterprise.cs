using System;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesCatalog
    {
        public static readonly FeatureDefinition WebFarmCache = new FeatureDefinition(
            "WebFarmCache",
            new Guid("11c0fedf-16a7-4596-900c-59e860b47965"),
            "Web Farm Cache",
            false,
            false,
            "Enables WebFarm Cache use in Dnn", FeaturesCatalogRules.Security0Improved,
            BuildRule(BuiltIn.WebFarmCache, true)
        );

    }
}
