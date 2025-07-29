using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

internal partial class DbContentType(DbStorage.DbStorage db) : DbPartBase(db, "Db.Type")
{
    private TsDynDataContentType? TryGetTypeByStaticNameUntracked(string staticName)
        => DbContext.SqlDb.TsDynDataContentTypes
            .AsNoTracking()
            .FirstOrDefault(a =>
                a.AppId == DbContext.AppId
                && a.StaticName == staticName && a.TransDeletedId == null
            );
}