namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbPartBase: HelperBase
{
    protected DbStorage.DbStorage DbContext { get; }

    internal DbPartBase(DbStorage.DbStorage dbStorage, string logName): base(dbStorage.Log, logName)
    {
        // Don't connect service - this was initialized before...
        DbContext = dbStorage;
    }
}