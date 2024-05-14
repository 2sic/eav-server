using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppLoader: IHasLog
{
    IAppStateBuilder LoadFullAppState();
}