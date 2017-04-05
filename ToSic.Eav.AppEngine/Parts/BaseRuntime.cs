namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class BaseRuntime
    {
        internal readonly AppRuntime _app;
        internal BaseRuntime(AppRuntime app)
        {
            _app = app;
        }
        

    }
}
