using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
{
    private readonly Generator<DbStorage, StorageOptions> _dbLazy = null!;

    [field: AllowNull, MaybeNull]
    public DbStorage DbStorage => field ??= _dbLazy.New(new(AppReader));

    public AppWorkCtxWithDb(Generator<DbStorage, StorageOptions> dbLazy, IAppReader appReader) : base(appReader)
    {
        _dbLazy = dbLazy;
    }

    public AppWorkCtxWithDb(DbStorage db, IAppReader appReader) : base(appReader)
    {
        DbStorage = db;
    }

}