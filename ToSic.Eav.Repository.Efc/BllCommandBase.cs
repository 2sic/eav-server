using ToSic.Lib.Services;


namespace ToSic.Eav.Repository.Efc
{
    public class BllCommandBase: HelperBase
    {
        protected DbDataController DbContext { get; }

        internal BllCommandBase(DbDataController dataController, string logName): base(dataController.Log, logName)
        {
            // Don't connect service - this was initialized before...
            DbContext = dataController;
        }
    }
}
