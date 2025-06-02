using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppStateLoader: IHasLog
{
    IAppStateBuilder LoadFullAppState(LogSettings logSettings);
}