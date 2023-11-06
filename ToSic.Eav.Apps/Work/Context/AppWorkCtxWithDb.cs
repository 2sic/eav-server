using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Work
{
    public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
    {
        private readonly DbDataController _db;
        private readonly LazySvc<DbDataController> _dbLazy;

        public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, AppState appState) : base(appState)
        {
            _dbLazy = dbLazy;
        }
        public AppWorkCtxWithDb(DbDataController db, AppState appState) : base(appState)
        {
            _db = db;
        }

        public DbDataController DataController => _db ?? _dbLazy.Value;
    }
}