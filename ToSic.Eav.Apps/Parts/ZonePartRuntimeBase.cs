using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class ZonePartRuntimeBase : HasLog
    {
        protected internal ZoneRuntime ZoneRuntime { get; internal set; }

        protected ZonePartRuntimeBase(string logName): base(logName)
        {
        }

    }

    public static class ZonePartExtensions
    {
        public static T ConnectTo<T>(this T parent, ZoneRuntime zoneRuntime) where T : ZonePartRuntimeBase => parent.Log.WrpFn<T>(_ =>
        {
            parent.ZoneRuntime = zoneRuntime;
            return parent;
        });
    }
}
