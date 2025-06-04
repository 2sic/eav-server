using ToSic.Sys.Boot;

namespace ToSic.Sys.Capabilities.Features;

internal class BootRegistrationEavFeatures(FeaturesCatalog featuresCatalog)
    : BootProcessBase("EavFts", bootPhase: BootPhase.Registrations, connect: [featuresCatalog])
{
    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public override void Run() => RegisterBuiltInSysFeatures.Register(featuresCatalog);
}