using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Persistence.Sys.AppState;
using ToSic.Eav.Persistence.Sys.Loaders;

namespace ToSic.Eav.Repositories;

/// <summary>
/// Core repository loader, used by the AppCache to self-load zones and apps.
/// </summary>
/// <remarks>
/// For more advanced operations, especially when creating new apps, use `IRepositoryLoaderAndCreator`.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRepositoryLoader: IHasLog, IContentTypeLoader
{
    /// <summary>
    /// will enforce that app settings etc. are created
    /// </summary>
    /// <param name="appId">AppId (can be different from the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <param name="codeRefTrail">CodeRef of the original caller to know where it came from</param>
    /// <returns></returns>
    IAppStateCache AppState(int appId, CodeRefTrail codeRefTrail);

    IAppStateCache Update(IAppStateCache app, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds = null);


    IDictionary<int, Zone> Zones();

    string PrimaryLanguage { get; set; }
}