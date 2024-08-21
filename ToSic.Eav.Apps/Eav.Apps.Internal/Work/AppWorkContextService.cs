using ToSic.Eav.Apps.State;
using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Helper to prepare the work context of any Unit of Work
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkContextService(
    LazySvc<IDataSourcesService> dataSourceSvc,
    LazySvc<IAppReaders> appReaders,
    Generator<LazySvc<DbDataController>> dbGen)
    : ServiceBase("App.WrkCtx", connect: [appReaders, dataSourceSvc, dbGen])
{
    /// <summary>
    /// In rare cases this is helpful outside; for now we surface it, may change later
    /// </summary>
    public IAppReaders AppReaders => appReaders.Value;
    
    public IAppWorkCtx Context(IAppReader appState) => new AppWorkCtx(appState);

    public IAppWorkCtxPlus ContextPlus(IAppReader appState, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appState, showDrafts, data);

    public IAppWorkCtx Context(int appId) => new AppWorkCtx(appReaders.Value.GetReader(appId));
    public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appState: appReaders.Value.GetReader(appId), showDrafts, data);

    public IAppWorkCtx Context(IAppIdentity appIdentity) => new AppWorkCtx(appReaders.Value.KeepOrGetReader(appIdentity));
    public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appReaders.Value.KeepOrGetReader(appIdentity), showDrafts, data);

    public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(appCtx, dataSourceSvc.Value, appCtx.AppReader, showDrafts, data);

    public IAppWorkCtxWithDb CtxWithDb(IAppIdentity identity) => CtxWithDb(Context(identity).AppReader);

    public IAppWorkCtxWithDb CtxWithDb(IAppReader appState, DbDataController existingDb = default)
        => existingDb == null
            ? new(dbGen.New().SetInit(dc => dc.Init(appState)), appState)
            : new AppWorkCtxWithDb(existingDb, appState);

}