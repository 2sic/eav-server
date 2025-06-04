using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Repositories;

public interface IAppsAndZonesLoaderWithRaw: IAppsAndZonesLoader
{
    /// <summary>
    /// Special loader which won't initialize the state.
    /// We're creating an own API for this, to better track down where things come from in case something is surprisingly wrong.
    /// </summary>
    /// <param name="appId">AppId (can be different from the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <returns></returns>
    /// <param name="codeRefTrail">CodeRef of the original caller to know where it came from</param>
    IAppStateBuilder AppStateRawBuilder(int appId, CodeRefTrail codeRefTrail);

    IAppReader AppReaderRaw(int appId, CodeRefTrail codeRefTrail);
}
