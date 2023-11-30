using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Internal.Loaders;

public interface IAppLoader: IHasLog
{
    AppStateBuilder LoadFullAppState();
}