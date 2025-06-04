using ToSic.Sys.Capabilities.Licenses;
using static ToSic.Sys.Capabilities.Licenses.BuiltInLicenses;

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