using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Experimental: Base class for all dependencies helpers.
    ///
    /// Can collect all objects which need the log and init that.
    /// </summary>
    /// <typeparam name="TDependencies"></typeparam>
    public class DependenciesBase<TDependencies> where TDependencies : DependenciesBase<TDependencies>
    {
        /// <summary>
        /// Special helper to keep track of all dependencies which need a log, to init once SetLog is called
        /// </summary>
        private DependencyLogs DependencyLogs { get; } = new DependencyLogs();

        /// <summary>
        /// Add objects to various queues to be auto-initialized when <see cref="SetLog"/> is called later on
        /// </summary>
        /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
        protected void AddToLogQueue(params object[] services) => DependencyLogs.Add(services);

        /// <summary>
        /// Auto-initialize the log on all dependencies.
        /// Special format to allow command chaining, so it returns itself.
        /// </summary>
        public virtual TDependencies SetLog(ILog log)
        {
            if (InitDone) return this as TDependencies;
            DependencyLogs.SetLog(log);
            InitDone = true;
            return this as TDependencies;
        }
        internal bool InitDone;

    }
}
