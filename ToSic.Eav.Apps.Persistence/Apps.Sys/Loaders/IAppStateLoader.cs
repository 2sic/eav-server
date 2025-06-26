using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

namespace ToSic.Eav.Apps.Sys.Loaders;

public interface IAppStateLoader: IHasLog
{
    IAppStateBuilder LoadFullAppState(LogSettings logSettings);
}