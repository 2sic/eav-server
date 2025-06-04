using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Persistence.Sys.AppState;

namespace ToSic.Eav.Repository.Efc.Sys;

/// <summary>
/// Wrapper for the EfcLoader, because we also need to do write operations on PrimaryApps, but the EFC loader cannot do that
/// </summary>
internal class EfcAppsAndZonesLoader(DbStorage.DbStorage dbStorage) : IAppsAndZonesLoaderWithRaw
{
    private readonly DbStorage.DbStorage _dbStorage = dbStorage.Init(null, null);

    public ILog Log => _dbStorage.Loader.Log;

    public IList<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source)
        => _dbStorage.Loader.ContentTypes(appId, source);

    public IAppStateBuilder AppStateRawBuilder(int appId, CodeRefTrail codeRefTrail)
        => _dbStorage.Loader.AppStateRawBuilder(appId, codeRefTrail);

    public IAppReader AppReaderRaw(int appId, CodeRefTrail codeRefTrail)
        => _dbStorage.Loader.AppReaderRaw(appId, codeRefTrail);

    public IAppStateCache AppState(int appId, CodeRefTrail codeRefTrail)
        => _dbStorage.Loader.AppState(appId, codeRefTrail);

    public IAppStateCache Update(IAppStateCache app, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds = null)
        => _dbStorage.Loader.Update(app, startAt, codeRefTrail, entityIds);

    public IDictionary<int, Zone> Zones()
    {
        _dbStorage.Zone.AddMissingPrimaryApps();
        return _dbStorage.Loader.Zones();
    }

    public string PrimaryLanguage
    {
        get => _dbStorage.Loader.PrimaryLanguage;
        set => _dbStorage.Loader.PrimaryLanguage = value;
    }
}