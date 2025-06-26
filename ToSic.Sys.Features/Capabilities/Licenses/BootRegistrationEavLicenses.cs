using ToSic.Sys.Boot;

namespace ToSic.Sys.Capabilities.Licenses;

internal class BootRegistrationEavLicenses(LicenseCatalog licenseCatalog)
    : BootProcessBase("EavLic", bootPhase: BootPhase.Registrations, connect: [licenseCatalog])
{
    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public override void Run() => RegisterEavLicenses.Register(licenseCatalog);
}