using ToSic.Eav.Internal.Licenses;

using static ToSic.Eav.Internal.Licenses.BuiltInLicenses;

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