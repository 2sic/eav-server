using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders;

internal class AppLoaderUnknown: ServiceBase, IAppLoader, IIsUnknown
{
    private readonly Generator<IAppStateBuilder> _stateBuilder;
    public AppLoaderUnknown(WarnUseOfUnknown<AppLoaderUnknown> _, Generator<IAppStateBuilder> stateBuilder) : base("Eav.BscRnt")
    {
        _stateBuilder = stateBuilder;
    }

    public IAppStateBuilder LoadFullAppState() => _stateBuilder.New().InitForPreset(); 
    // new(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, new Log(LogScopes.NotImplemented));

    public void ReloadConfigEntities() { /* do nothing */ }
}