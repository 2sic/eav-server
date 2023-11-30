using ToSic.Lib.Logging;
using static ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppLoader: IHasLog
{
    AppStateBuilder LoadFullAppState();
}