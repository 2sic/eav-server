﻿using ToSic.Eav.DataSource;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Helper to prepare the work context of any Unit of Work
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkContextService(
    LazySvc<IDataSourcesService> dataSourceSvc,
    LazySvc<IAppReaderFactory> appReaders,
    Generator<DbStorage, StorageOptions> dbGen)
    : ServiceBase("App.WrkCtx", connect: [appReaders, dataSourceSvc, dbGen])
{
    /// <summary>
    /// In rare cases this is helpful outside; for now we surface it, may change later
    /// </summary>
    public IAppReaderFactory AppReaders => appReaders.Value;
    

    public IAppWorkCtx Context(int appId)
        => new AppWorkCtx(appReaders.Value.Get(appId));

    public IAppWorkCtx Context(IAppReader appReader)
        => new AppWorkCtx(appReader);

    public IAppWorkCtx Context(IAppIdentity appIdentity)
        => new AppWorkCtx(appReaders.Value.GetOrKeep(appIdentity));


    public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource? data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appReader: appReaders.Value.Get(appId), showDrafts, data);

    public IAppWorkCtxPlus ContextPlus(IAppReader appReader, bool? showDrafts = default, IDataSource? data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appReader, showDrafts, data);

    public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource? data = default)
        => new AppWorkCtxPlus(dataSourceSvc.Value, appReaders.Value.GetOrKeep(appIdentity), showDrafts, data);

    public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource? data = default)
        => new AppWorkCtxPlus(appCtx, dataSourceSvc.Value, appCtx.AppReader, showDrafts, data);

    public IAppWorkCtxWithDb CtxWithDb(IAppIdentity identity)
        => CtxWithDb(Context(identity).AppReader);

    public IAppWorkCtxWithDb CtxWithDb(IAppReader appReader, DbStorage? existingDb = default)
        => existingDb == null
            ? new(dbGen, appReader)
            : new AppWorkCtxWithDb(existingDb, appReader);

}