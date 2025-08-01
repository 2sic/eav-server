using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

internal partial class DbContentType(DbStorage.DbStorage db) : DbPartBase(db, "Db.Type")
{
    private TsDynDataContentType? TryGetTypeByStaticTracked(string staticName)
        => DbStore.SqlDb.TsDynDataContentTypes
            .FirstOrDefault(a =>
                a.AppId == DbStore.AppId
                && a.StaticName == staticName && a.TransDeletedId == null
            );
}