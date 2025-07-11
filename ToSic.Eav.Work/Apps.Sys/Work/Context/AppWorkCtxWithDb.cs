﻿using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWorkCtxWithDb : AppWorkCtx, IAppWorkCtxWithDb
{
    private readonly LazySvc<DbStorage>? _dbLazy;

    [field: AllowNull, MaybeNull]
    public DbStorage DbStorage => field ??= _dbLazy!.Value;

    public AppWorkCtxWithDb(LazySvc<DbStorage> dbLazy, IAppReader appReader) : base(appReader)
    {
        _dbLazy = dbLazy;
    }

    public AppWorkCtxWithDb(DbStorage db, IAppReader appReader) : base(appReader)
    {
        DbStorage = db;
    }

}