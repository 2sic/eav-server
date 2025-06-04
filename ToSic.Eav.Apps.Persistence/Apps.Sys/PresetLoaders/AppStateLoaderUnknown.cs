using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Internal.Loaders;

internal class AppStateLoaderUnknown(WarnUseOfUnknown<AppStateLoaderUnknown> _, Generator<IAppStateBuilder> stateBuilder): ServiceBase("Eav.BscRnt"), IAppStateLoader, IIsUnknown
{
    public IAppStateBuilder LoadFullAppState(LogSettings logSettings)
        => stateBuilder.New().InitForPreset(); 
}