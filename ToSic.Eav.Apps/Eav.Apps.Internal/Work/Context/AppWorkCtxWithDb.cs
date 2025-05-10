using ToSic.Eav.Apps.State;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
{
    private readonly LazySvc<DbDataController> _dbLazy;

    //public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, AppState appState) : base(appState)
    //{
    //    _dbLazy = dbLazy;
    //}
    public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, IAppReader appState) : base(appState)
    {
        _dbLazy = dbLazy;
    }
    //public AppWorkCtxWithDb(DbDataController db, AppState appState) : base(appState)
    //{
    //    _db = db;
    //}
    public AppWorkCtxWithDb(DbDataController db, IAppReader appState) : base(appState)
    {
        DataController = db;
    }

    public DbDataController DataController => field ?? _dbLazy.Value;
}