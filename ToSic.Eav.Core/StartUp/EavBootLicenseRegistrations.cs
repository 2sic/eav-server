using ToSic.Lib.Boot;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.StartUp;

internal class EavBootLicenseRegistrations(LicenseCatalog licenseCatalog)
    : BootProcessBase("EavLic", bootPhase: BootPhase.Registrations, connect: [licenseCatalog])
{
    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public override void Run() => RegisterEavLicenses.Register(licenseCatalog);
}