using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

internal partial class DbContentType(DbStorage.DbStorage db) : DbPartBase(db, "Db.Type")
{
    private TsDynDataContentType GetTypeByStaticName(string staticName)
    {
        return DbContext.SqlDb.TsDynDataContentTypes.FirstOrDefault(a =>
            a.AppId == DbContext.AppId && a.StaticName == staticName && a.TransDeletedId == null
        );
    }
        
}