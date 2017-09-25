using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class ManagerBase
    {
        protected Log Log = new Log("ManBas");

        internal readonly AppManager _appManager;
        internal ManagerBase(AppManager app, Log parentLog, string logRename = null)
        {
            _appManager = app;
            Log.LinkTo(parentLog);
            if(logRename != null)
                Log.Rename(logRename);
        }
        

    }
}
