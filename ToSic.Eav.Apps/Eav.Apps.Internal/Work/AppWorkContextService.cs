using ToSic.Eav.Apps.State;
using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Helper to prepare the work context of any Unit of Work
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkContextService: ServiceBase
{
    public AppWorkContextService(LazySvc<IDataSourcesService> dataSourceSvc, LazySvc<IAppStates> appStates, Generator<LazySvc<DbDataController>> dbGen) : base("App.WrkCtx")
    {
        ConnectServices(
            _appStates = appStates,
            _dataSourceSvc = dataSourceSvc,
            _dbGen = dbGen
        );
    }

    private readonly Generator<LazySvc<DbDataController>> _dbGen;
    private readonly LazySvc<IAppStates> _appStates;
    private readonly LazySvc<IDataSourcesService> _dataSourceSvc;

    /// <summary>
    /// In rare cases this is helpful outside; for now we surface it, may change later
    /// </summary>
    public IAppStates AppStates => _appStates.Value;
    
    public IAppWorkCtx Context(IAppState appState) => new AppWorkCtx(appState);

    public IAppWorkCtxPlus ContextPlus(IAppState appState, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, appState, showDrafts, data);

    public IAppWorkCtx Context(int appId) => new AppWorkCtx(_appStates.Value.GetReader(appId));
    public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, appState: _appStates.Value.GetReader(appId), showDrafts, data);

    public IAppWorkCtx Context(IAppIdentity appIdentity) => new AppWorkCtx(_appStates.Value.KeepOrGetReader(appIdentity));
    public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, _appStates.Value.KeepOrGetReader(appIdentity), showDrafts, data);

    public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(appCtx, _dataSourceSvc.Value, appCtx.AppState, showDrafts, data);

    public IAppWorkCtxWithDb CtxWithDb(IAppIdentity identity) => CtxWithDb(Context(identity).AppState);

    public IAppWorkCtxWithDb CtxWithDb(IAppStateInternal appState, DbDataController existingDb = default)
        => existingDb == null
            ? new AppWorkCtxWithDb(_dbGen.New().SetInit(dc => dc.Init(appState)), appState)
            : new AppWorkCtxWithDb(existingDb, appState);

}