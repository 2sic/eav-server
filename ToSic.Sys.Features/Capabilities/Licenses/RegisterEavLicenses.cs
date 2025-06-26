﻿using static ToSic.Sys.Capabilities.Licenses.BuiltInLicenses;

namespace ToSic.Sys.Capabilities.Licenses;

internal class RegisterEavLicenses
{
    public static void Register(LicenseCatalog cat)
        => cat.Register(
            CoreFree,
            CorePlus,
            CoreBeta,
            PatronBasic,
            PatronLanguages,
            PatronData,
            PatronAdvancedCms,
            PatronPerfectionist,
            PatronSentinel,
            PatronSuperAdmin,
            PatronPerformance, // new v20
            PatronInfrastructure,
            WebFarmCache,
            EnterpriseCms,

            BuiltInLicenses.System,
            Extension

#if DEBUG
            // disable in production
            ,
            CoreTesting
#endif
        );
}