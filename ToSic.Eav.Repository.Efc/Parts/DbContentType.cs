namespace ToSic.Eav.Repository.Efc.Parts;

internal partial class DbContentType(DbDataController db) : DbPartBase(db, "Db.Type")
{
    private ToSicEavAttributeSets GetTypeByStaticName(string staticName)
    {
        return DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(a =>
            a.AppId == DbContext.AppId && a.StaticName == staticName && a.TransactionIdDeleted == null
        );
    }
        
}