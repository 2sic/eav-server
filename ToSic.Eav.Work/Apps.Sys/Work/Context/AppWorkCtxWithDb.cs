using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
{
    private readonly LazySvc<DbDataController> _dbLazy;

    public DbDataController DataController => field ??= _dbLazy.Value;

    public AppWorkCtxWithDb(LazySvc<DbDataController> dbLazy, IAppReader appReader) : base(appReader)
    {
        _dbLazy = dbLazy;
    }

    public AppWorkCtxWithDb(DbDataController db, IAppReader appReader) : base(appReader)
    {
        DataController = db;
    }

}