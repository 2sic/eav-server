using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForBeta = BuildRule(Licenses.BuiltInLicenses.CoreBeta, false);
        public static List<FeatureLicenseRule> ForBetaEnabled = BuildRule(Licenses.BuiltInLicenses.CoreBeta, true);


        // TODO: Probably change how this setting is used, so it's "Enable..." and defaults to true ?
        // ATM only used in azing, so still easy to change

        public static readonly FeatureDefinition BlockFileResolveOutsideOfEntityAdam = new FeatureDefinition(
            "BlockFileResolveOutsideOfEntityAdam-BETA",
            new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
            "BlockFileResolveOutsideOfEntityAdam (BETA, not final)",
            false,
            false,
            "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.",
            FeaturesCatalogRules.Security0Improved,
            ForBeta
        );

    }
}
