using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DI
{
    public class LazyInitLog<T>: LazyInit<T> where T: class, IHasLog
    {
        public LazyInitLog(Lazy<T> valueLazy) : base(valueLazy) { }

        public LazyInitLog<T> SetLog(ILog parentLog)
        {
            SetInit(x => x.Log.LinkTo(parentLog));
            return this;
        }
    }
}
