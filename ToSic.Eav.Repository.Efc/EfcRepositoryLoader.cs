using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Repository.Efc;

/// <summary>
/// Wrapper for the EfcLoader, because we also need to do write operations on PrimaryApps, but the EFC loader cannot do that
/// </summary>
internal class EfcRepositoryLoader(DbDataController dataController) : IRepositoryLoader
{
    private readonly DbDataController _dataController = dataController.Init(null, null);

    public ILog Log => _dataController.Loader.Log;

    public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
        => _dataController.Loader.ContentTypes(appId, source);

    public IAppStateBuilder AppStateBuilderRaw(int appId, CodeRefTrail codeRefTrail)
        => _dataController.Loader.AppStateBuilderRaw(appId, codeRefTrail);

    public IAppStateCache AppStateInitialized(int appId, CodeRefTrail codeRefTrail)
        => _dataController.Loader.AppStateInitialized(appId, codeRefTrail);

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