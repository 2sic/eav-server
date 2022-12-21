using System;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Enables generating additional objects of a specific type
    /// </summary>
    public class Generator<T>: IGenerator<T>, IHasLog, ILazyInitLog
    {
        public Generator(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        public T New()
        {
            var created = _sp.Build<T>();
            if (created is IHasLog withLog) withLog.Init(Log);
            return created;
        }

        void ILazyInitLog.SetLog(ILog parentLog) => Log = parentLog;

        /// <summary>
        /// The parent log.
        /// </summary>
        public ILog Log { get; private set; }

    }
}
