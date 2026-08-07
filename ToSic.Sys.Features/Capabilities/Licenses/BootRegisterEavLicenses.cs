using ToSic.Sys.Boot;
using static ToSic.Sys.Capabilities.Licenses.BuiltInLicenses;

namespace ToSic.Sys.Capabilities.Licenses;

internal sealed class BootRegisterEavLicenses(LicenseCatalog licenseCatalog)
    : BootProcessBase("EavLic", bootPhase: BootPhase.Registrations, connect: [licenseCatalog])
{
    /// <summary>
    /// Implementation of boot to register licenses
    /// </summary>
    public override void Run() => licenseCatalog.Register(EavFeatureSets);

    /// <summary>
    /// List of all feature sets / licenses.
    /// </summary>
    private static readonly FeatureSet.FeatureSet[] EavFeatureSets =
    [
        CoreFree,
        CorePlus,
        CoreBeta,
        DevCoreFree,
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
        Extension,

        // Test features, disable in production
#if DEBUG
        CoreTesting,
#endif
    ];

}