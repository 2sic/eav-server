using ToSic.Lib.Logging;


namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class ZonePartRuntimeBase<TRuntime, T> : HasLog 
        where TRuntime: ZoneRuntime 
        where T : ZonePartRuntimeBase<TRuntime, T>
    {
        protected TRuntime ZoneRuntime { get; private set; }

        protected ZonePartRuntimeBase(string logName): base(logName)
        {
        }

        public T Init(TRuntime zoneRuntime, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            ZoneRuntime = zoneRuntime;
            return this as T;
        }

    }
}
