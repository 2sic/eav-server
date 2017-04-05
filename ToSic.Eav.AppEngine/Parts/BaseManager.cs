namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class BaseManager
    {
        internal readonly AppManager _appManager;
        internal BaseManager(AppManager app)
        {
            _appManager = app;
        }
        

    }
}
