using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders;

internal class AppLoaderUnknown(WarnUseOfUnknown<AppLoaderUnknown> _, Generator<IAppStateBuilder> stateBuilder): ServiceBase("Eav.BscRnt"), IAppLoader, IIsUnknown
{
    public IAppStateBuilder LoadFullAppState(LogSettings logSettings)
        => stateBuilder.New().InitForPreset(); 
}