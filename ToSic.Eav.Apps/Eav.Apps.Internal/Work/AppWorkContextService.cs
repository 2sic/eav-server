﻿using ToSic.Eav.Apps.State;
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
    LazySvc<IAppStates> appStates,
    Generator<LazySvc<DbDataController>> dbGen)
    : ServiceBase("App.WrkCtx", connect: [appStates, dataSourceSvc, dbGen])
{
    /// <summary>
    /// In rare cases this is helpful outside; for now we surface it, may change later
    /// </summary>
    public IAppStates AppStates => appStates.Value;
    
    public IAppWorkCtx Context(IAppState appState) => new AppWorkCtx(appState);

    public IAppWorkCtxPlus ContextPlus(IAppState appState, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appState, showDrafts, data);

    public IAppWorkCtx Context(int appId) => new AppWorkCtx(appStates.Value.GetReader(appId));
    public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appState: appStates.Value.GetReader(appId), showDrafts, data);

    public IAppWorkCtx Context(IAppIdentity appIdentity) => new AppWorkCtx(appStates.Value.KeepOrGetReader(appIdentity));
    public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appStates.Value.KeepOrGetReader(appIdentity), showDrafts, data);

    public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
        => new AppWorkCtxPlus(appCtx, dataSourceSvc.Value, appCtx.AppState, showDrafts, data);

    public IAppWorkCtxWithDb CtxWithDb(IAppIdentity identity) => CtxWithDb(Context(identity).AppState);

    public IAppWorkCtxWithDb CtxWithDb(IAppStateInternal appState, DbDataController existingDb = default)
        => existingDb == null
            ? new(dbGen.New().SetInit(dc => dc.Init(appState)), appState)
            : new AppWorkCtxWithDb(existingDb, appState);

}