using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Helper to prepare the work context of any Unit of Work
/// </summary>
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

    //public TCtx CtxGen<TCtx>(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default) where TCtx : class, IAppWorkCtx
    //{
    //    var baseCtx = Ctx(ctx, identity, state, appId);
    //    if (typeof(TCtx) == typeof(IAppWorkCtxPlus)) return ToCtxPlus(baseCtx) as TCtx;
    //    if (typeof(TCtx) == typeof(IAppWorkCtxWithDb)) return CtxWithDb(baseCtx.AppState) as TCtx;
    //    if (typeof(TCtx) == typeof(IAppWorkCtx)) return baseCtx as TCtx;
    //    throw new ArgumentException($"Type for {nameof(TCtx)} unknown");
    //}

    //public IAppWorkCtx Ctx(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default)
    //{
    //    if (ctx != null) return ctx;
    //    if (state != null) return Context(state);
    //    if (identity != null) return Context(identity);
    //    if (appId != null) return Context(appId.Value);
    //    throw new ArgumentException("Some of the identity arguments must be provided.");
    //}


    //public IAppWorkCtxPlus CtxPlus(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default)
    //{
    //    if (ctx is IAppWorkCtxPlus asPlus) return asPlus;
    //    if (ctx != null) return ContextPlus(ctx.AppState);
    //    if (state != null) return ContextPlus(state);
    //    if (identity != null) return ContextPlus(identity);
    //    if (appId != null) return ContextPlus(appId.Value);
    //    throw new ArgumentException("Some of the identity arguments must be provided.");
    //}


    public IAppWorkCtx Context(AppState appState) => new AppWorkCtx(appState);
    public IAppWorkCtxPlus ContextPlus(AppState appState, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, appState, showDrafts, data);

    public IAppWorkCtx Context(int appId) => new AppWorkCtx(_appStates.Value.Get(appId));
    public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, appState: _appStates.Value.Get(appId), showDrafts, data);

    public IAppWorkCtx Context(IAppIdentity appIdentity) => new AppWorkCtx(_appStates.Value.KeepOrGet(appIdentity));
    public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(_dataSourceSvc.Value, _appStates.Value.KeepOrGet(appIdentity), showDrafts, data);

    public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(appCtx, _dataSourceSvc.Value, appCtx.AppState, showDrafts, data);

    public IAppWorkCtxWithDb CtxWithDb(IAppIdentity identity) => CtxWithDb(Context(identity).AppState);

    public IAppWorkCtxWithDb CtxWithDb(AppState appState, DbDataController existingDb = default)
        => existingDb == null
            ? new AppWorkCtxWithDb(_dbGen.New().SetInit(dc => dc.Init(appState)), appState)
            : new AppWorkCtxWithDb(existingDb, appState);
}