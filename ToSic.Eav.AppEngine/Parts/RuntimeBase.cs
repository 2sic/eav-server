using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public abstract class RuntimeBase: HasLog
    {
        internal readonly AppRuntime App;
        internal RuntimeBase(AppRuntime app, Log parentLog): base("App.RunTB", parentLog)
        {
            App = app;
        }
        

    }
}
