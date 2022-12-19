using System;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Enables generating additional objects of a specific type
    /// </summary>
    public class GeneratorLog<T>: IGenerator<T>, ILazyInitLog where T : class, IHasLog
    {
        public GeneratorLog(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        public T New()
        {
            var created = _sp.Build<T>();
            return created.Init(_parentLog);
        }

        public GeneratorLog<T> SetLog(ILog parentLog)
        {
            _parentLog = parentLog;
            return this;
        }

        void ILazyInitLog.SetLog(ILog parentLog) => _parentLog = parentLog;

        private ILog _parentLog;

    }
}
