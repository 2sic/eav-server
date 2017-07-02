namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class RuntimeBase
    {
        internal readonly AppRuntime App;
        internal RuntimeBase(AppRuntime app)
        {
            App = app;
        }
        

    }
}
