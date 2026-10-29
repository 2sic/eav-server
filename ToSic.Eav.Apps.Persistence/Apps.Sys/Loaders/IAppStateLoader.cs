using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

namespace ToSic.Eav.Apps.Sys.Loaders;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppStateLoader: IHasLog
{
    IAppStateBuilder LoadFullAppState(ToSic.Sys.Logging.LogSettings logSettings);
}