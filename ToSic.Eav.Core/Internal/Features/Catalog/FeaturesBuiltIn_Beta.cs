using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForBeta = BuildRule(BuiltInLicenses.CoreBeta, false);
    public static List<FeatureLicenseRule> ForBetaEnabled = BuildRule(BuiltInLicenses.CoreBeta, true);

    // ATM only used in azing, so still easy to change

    public static readonly Feature AdamRestrictLookupToEntity = new(
        nameof(AdamRestrictLookupToEntity),
        new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852"),
        "ADAM - Restrict file lookup to the same entity (BETA, not final)",
        false,
        false,
        "If enabled, then links like 'file:72' will only work if the file is inside the ADAM of the current Entity.",
        FeaturesCatalogRules.Security0Improved,
        ForBeta
    );

}