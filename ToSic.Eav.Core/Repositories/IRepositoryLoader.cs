using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Loaders;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Repositories;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRepositoryLoader: IHasLog, IContentTypeLoader
{
    /// <summary>
    /// Special loader which won't initialize the state.
    /// We're creating an own API for this, to better track down where things come from in case something is surprisingly wrong.
    /// </summary>
    /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <returns></returns>
    /// <param name="codeRefTrail">CodeRef of the original caller to know where it came from</param>
    AppState AppStateRaw(int appId, CodeRefTrail codeRefTrail);

    /// <summary>
    /// will enforce that app settings etc. are created
    /// </summary>
    /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <param name="codeRefTrail">CodeRef of the original caller to know where it came from</param>
    /// <returns></returns>
    AppState AppStateInitialized(int appId, CodeRefTrail codeRefTrail);

    AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null);


    IDictionary<int, Zone> Zones();

    string PrimaryLanguage { get; set; }
}