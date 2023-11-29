using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
{
    private readonly DbDataController _db;
    private readonly LazySvc<DbDataController> _dbLazy;

    //public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, AppState appState) : base(appState)
    //{
    //    _dbLazy = dbLazy;
    //}
    public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, IAppStateInternal appState) : base(appState)
    {
        _dbLazy = dbLazy;
    }
    //public AppWorkCtxWithDb(DbDataController db, AppState appState) : base(appState)
    //{
    //    _db = db;
    //}
    public AppWorkCtxWithDb(DbDataController db, IAppStateInternal appState) : base(appState)
    {
        _db = db;
    }

    public DbDataController DataController => _db ?? _dbLazy.Value;
}