namespace ToSic.Eav.Repository.Efc.Parts;

internal class DbPartBase: HelperBase
{
    protected DbDataController DbContext { get; }

    internal DbPartBase(DbDataController dataController, string logName): base(dataController.Log, logName)
    {
        // Don't connect service - this was initialized before...
        DbContext = dataController;
    }
}