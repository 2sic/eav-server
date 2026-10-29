using ToSic.Eav.DataSource;
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
    public IDisposable WithContext(IAppReader appReader)
    {
        return OverrideService<IAppWorkContext>.Use(ContextNew(appReader));
    }
    
    public IAppWorkContext ContextNew(IAppReader appReader, bool? showDrafts = default)
        => new AppWorkContext(appReader, this, showDrafts);
    public IAppWorkContext ContextNew(int appId, bool? showDrafts = default)
        => ContextNew(appReaders.Value.Get(appId), showDrafts);
    public IAppWorkContext ContextNew(IAppIdentity appIdentity, bool? showDrafts = default)
        => ContextNew(appReaders.Value.GetOrKeep(appIdentity), showDrafts);

    internal Generator<DbStorage, StorageOptions> DbGenerator => dbGen;
    internal LazySvc<IDataSourcesService> DataSourcesSvc => dataSourceSvc;

}