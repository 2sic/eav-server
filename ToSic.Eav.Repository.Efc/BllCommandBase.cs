using ToSic.Lib.Logging;
using ToSic.Lib.Services;


namespace ToSic.Eav.Repository.Efc
{
    public class BllCommandBase: ServiceBase
    {
        protected DbDataController DbContext { get; }

        internal BllCommandBase(DbDataController dataController, string logName): base(logName)
        {
            // Don't connect service - this was initialized before...
            DbContext = dataController;
            this.Init(dataController.Log);
            // Log = new Log(logName, dataController.Log);
        }
    }
}
