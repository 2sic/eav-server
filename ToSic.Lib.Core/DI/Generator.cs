using System;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Lazy generator to create multiple new services/objects of a specific type.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class Generator<T>: IHasLog, ILazyInitLog
    {
        /// <summary>
        /// Constructor should only be used in DI context and never be called directly.
        /// </summary>
        /// <param name="sp"></param>
        public Generator(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        /// <summary>
        /// Factory method to generate a new service
        /// </summary>
        /// <returns></returns>
        public T New() => _sp.Build<T>(Log);

        /// <summary>
        /// Initializer to attach the log to the generator.
        /// The log is later given to generated objects.
        /// </summary>
        /// <param name="parentLog"></param>
        void ILazyInitLog.SetLog(ILog parentLog) => Log = parentLog;

        /// <summary>
        /// The parent log, which is attached to newly generated objects
        /// _if_ they support logging.
        /// </summary>
        public ILog Log { get; private set; }

    }
}
