using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

internal class AppStateLoaderUnknown(WarnUseOfUnknown<AppStateLoaderUnknown> _, Generator<IAppStateBuilder> stateBuilder): ServiceBase("Eav.BscRnt"), IAppStateLoader, IIsUnknown
{
    public IAppStateBuilder LoadFullAppState(LogSettings logSettings)
        => stateBuilder.New().InitForPreset(); 
}