using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class ZonePartRuntimeBase<TRuntime, T> : HasLog 
        where TRuntime: ZoneRuntime 
        where T : ZonePartRuntimeBase<TRuntime, T>
    {
        public IServiceProvider ServiceProvider { get; }
        protected TRuntime ZoneRuntime { get; private set; }

        protected ZonePartRuntimeBase(IServiceProvider serviceProvider, string logName): base(logName)
        {
            ServiceProvider = serviceProvider;
        }

        //protected ZonePartRuntimeBase(TRuntime zoneRuntime, ILog parentLog, string logName = null): base(logName ?? "App.RunTB", parentLog)
        //{
        //    ZoneRuntime = zoneRuntime;
        //}

        public T Init(TRuntime zoneRuntime, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            ZoneRuntime = zoneRuntime;
            return this as T;
        }

    }
}
