namespace ToSic.Eav.Repository.Efc.Parts;

internal partial class DbContentType: DbPartBase
{
    public DbContentType(DbDataController db) : base(db, "Db.Type") {}


    private ToSicEavAttributeSets GetTypeByStaticName(string staticName)
    {
        return DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(a =>
            a.AppId == DbContext.AppId && a.StaticName == staticName && a.ChangeLogDeleted == null
        );
    }
        
}