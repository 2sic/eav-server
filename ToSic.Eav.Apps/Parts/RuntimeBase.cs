using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class RuntimeBase: HasLog
    {
        // ReSharper disable once InconsistentNaming
        protected readonly AppRuntime AppRT;
        protected RuntimeBase(AppRuntime appRt, ILog parentLog, string logName = null): base(logName ?? "App.RunTB", parentLog)
        {
            AppRT = appRt;
        }
        

    }
}
