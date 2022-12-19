using System;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Enables generating additional objects of a specific type
    /// </summary>
    public class GeneratorLog<T>: IGenerator<T>, IHasLog, ILazyInitLog where T : class, IHasLog
    {
        public GeneratorLog(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        public T New()
        {
            var created = _sp.Build<T>();
            return created.Init(Log);
        }

        public GeneratorLog<T> SetLog(ILog parentLog)
        {
            Log = parentLog;
            return this;
        }

        void ILazyInitLog.SetLog(ILog parentLog) => Log = parentLog;

        /// <summary>
        /// The parent log.
        /// </summary>
        public ILog Log { get; private set; }

    }
}
