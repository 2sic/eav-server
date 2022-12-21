using System;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    public class LazyInitLog<T>: LazyInit<T>, ILazyInitLog where T: class, IHasLog
    {
        public LazyInitLog(Lazy<T> valueLazy) : base(valueLazy) { }

        public LazyInitLog<T> SetLog(ILog parentLog)
        {
            (this as ILazyInitLog).SetLog(parentLog);
            return this;
        }
    }
}
