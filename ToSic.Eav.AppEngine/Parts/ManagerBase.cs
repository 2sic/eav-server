namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class ManagerBase
    {
        internal readonly AppManager _appManager;
        internal ManagerBase(AppManager app)
        {
            _appManager = app;
        }
        

    }
}
