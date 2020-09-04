using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repository.Efc
{
    public class BllCommandBase
    {
        protected DbDataController DbContext { get; }

        internal BllCommandBase(DbDataController dataController, string logName)
        {
            DbContext = dataController;
            Log = new Log(logName, dataController.Log);
        }

        protected readonly ILog Log;

    }
}
