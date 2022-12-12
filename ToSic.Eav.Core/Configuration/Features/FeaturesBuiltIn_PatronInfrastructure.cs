using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        internal static List<FeatureLicenseRule> ForPatronInfrastructureAutoEnabled = BuildRule(Licenses.BuiltInLicenses.PatronAdvancedCms, true);


        public static readonly FeatureDefinition SqlCompressDataTimeline = new FeatureDefinition(
            nameof(SqlCompressDataTimeline),
            new Guid("87325de8-d671-4731-bd58-186ff6de6329"),
            "Shrink your DB size by up to 80%. Enables compressed JSON for the change-history which is rarely accessed.",
            false,
            true,
            "todo",
            FeaturesCatalogRules.Security0Neutral,
            ForPatronInfrastructureAutoEnabled
        );

    }
}
