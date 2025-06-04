using ToSic.Eav.Apps.State;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Repository.Efc;

/// <summary>
/// Wrapper for the EfcLoader, because we also need to do write operations on PrimaryApps, but the EFC loader cannot do that
/// </summary>
internal class EfcRepositoryLoader(DbDataController dataController) : IRepositoryLoaderWithRaw
{
    private readonly DbDataController _dataController = dataController.Init(null, null);

    public ILog Log => _dataController.Loader.Log;

    public IList<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source)
        => _dataController.Loader.ContentTypes(appId, source);

    public IAppStateBuilder AppStateRawBuilder(int appId, CodeRefTrail codeRefTrail)
        => _dataController.Loader.AppStateRawBuilder(appId, codeRefTrail);

    public IAppReader AppReaderRaw(int appId, CodeRefTrail codeRefTrail)
        => _dataController.Loader.AppReaderRaw(appId, codeRefTrail);

    public IAppStateCache AppState(int appId, CodeRefTrail codeRefTrail)
        => _dataController.Loader.AppState(appId, codeRefTrail);

    public IAppStateCache Update(IAppStateCache app, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds = null)
        => _dataController.Loader.Update(app, startAt, codeRefTrail, entityIds);

    public IDictionary<int, Zone> Zones()
    {
        _dataController.Zone.AddMissingPrimaryApps();
        return _dataController.Loader.Zones();
    }

    public string PrimaryLanguage
    {
        get => _dataController.Loader.PrimaryLanguage;
        set => _dataController.Loader.PrimaryLanguage = value;
    }
}