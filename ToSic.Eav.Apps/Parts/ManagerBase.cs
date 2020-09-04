using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class ManagerBase
    {
        protected ILog Log = new Log("App.MBase");

        protected internal readonly AppManager AppManager;
        protected internal ManagerBase(AppManager app, ILog parentLog, string logRename = null)
        {
            AppManager = app;
            Log.LinkTo(parentLog);
            if(logRename != null)
                Log.Rename(logRename);
        }
        

    }
}
