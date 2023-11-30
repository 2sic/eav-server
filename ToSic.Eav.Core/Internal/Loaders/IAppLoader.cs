using ToSic.Eav.Apps;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppLoader: IHasLog
{
    AppState.AppStateBuilder LoadFullAppState();
}