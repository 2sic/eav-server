using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppStateLoader: IHasLog
{
    IAppStateBuilder LoadFullAppState(LogSettings logSettings);
}