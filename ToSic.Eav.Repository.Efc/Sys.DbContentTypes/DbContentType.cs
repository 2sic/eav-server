namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

internal partial class DbContentType(DbStorage.DbStorage dbStore) : HelperBase(dbStore.Log, "Db.Type")
{
    private TsDynDataContentType? TryGetTypeByStaticTracked(string staticName)
        => dbStore.SqlDb.TsDynDataContentTypes
            .FirstOrDefault(a =>
                a.AppId == dbStore.AppId
                && a.StaticName == staticName/* && a.TransDeletedId == null*/
            );
}