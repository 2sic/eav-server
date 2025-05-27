using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Lib.Services;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.StartUp;

internal class EavStartupRegistrations(FeaturesCatalog featuresCatalog, LicenseCatalog licenseCatalog)
    : ServiceBase($"{EavLogs.Eav}.SUpReg", connect: [featuresCatalog]), IStartUpRegistrations
{
    public string NameId => Log.NameId;

    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public void Register()
    {
        RegisterEavFeatures.Register(featuresCatalog);
        RegisterEavLicenses.Register(licenseCatalog);
    }
}