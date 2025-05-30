using ToSic.Lib.Boot;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.StartUp;

internal class EavBootFeaturesRegistrations(FeaturesCatalog featuresCatalog)
    : BootProcessBase("EavFts", bootPhase: BootPhase.Registrations, connect: [featuresCatalog])
{
    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public override void Run() => RegisterEavFeatures.Register(featuresCatalog);
}