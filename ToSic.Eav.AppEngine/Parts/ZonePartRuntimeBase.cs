using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class ZonePartRuntimeBase : HasLog
    {
        protected readonly ZoneRuntime ZoneRuntime;
        protected ZonePartRuntimeBase(ZoneRuntime zoneRuntime, ILog parentLog, string logName = null): base(logName ?? "App.RunTB", parentLog)
        {
            ZoneRuntime = zoneRuntime;
        }
        

    }
}
