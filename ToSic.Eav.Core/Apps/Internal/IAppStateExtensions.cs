using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Apps.State;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps;

public static class IAppStateExtensions
{
    public static IAppStateInternal Internal(this IAppState appState) => appState as IAppStateInternal;

    //public static IAppStateInternal ToInterface(this AppState appState, ILog log) => new AppStateReader(appState, log);

    public static IAppStateInternal ToInterface(this IAppStateCache appState, ILog log) => new AppStateReader(appState, log);
}