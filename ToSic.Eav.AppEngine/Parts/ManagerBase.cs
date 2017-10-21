using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class ManagerBase
    {
        protected Log Log = new Log("App.MBase");

        internal readonly AppManager AppManager;
        internal ManagerBase(AppManager app, Log parentLog, string logRename = null)
        {
            AppManager = app;
            Log.LinkTo(parentLog);
            if(logRename != null)
                Log.Rename(logRename);
        }
        

    }
}
