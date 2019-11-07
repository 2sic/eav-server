using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class RuntimeBase: HasLog
    {
        internal readonly AppRuntime App;
        internal RuntimeBase(AppRuntime app, ILog parentLog): base("App.RunTB", parentLog)
        {
            App = app;
        }
        

    }
}
