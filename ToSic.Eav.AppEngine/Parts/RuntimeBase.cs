using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class RuntimeBase: HasLog
    {
        protected readonly AppRuntime App;
        protected RuntimeBase(AppRuntime app, ILog parentLog, string logName = null): base(logName ?? "App.RunTB", parentLog)
        {
            App = app;
        }
        

    }
}
