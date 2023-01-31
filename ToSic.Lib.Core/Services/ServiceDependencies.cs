using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services
{
    /// <summary>
    /// Experimental: Base class for all dependencies helpers.
    ///
    /// Can collect all objects which need the log and init that.
    /// </summary>
    public abstract class ServiceDependencies: ILazyInitLog
    {
        /// <summary>
        /// Special helper to keep track of all dependencies which need a log, to init once SetLog is called
        /// </summary>
        internal DependencyLogs DependencyLogs { get; } = new DependencyLogs();

        /// <summary>
        /// Add objects to various queues to be auto-initialized when <see cref="ServiceDependenciesExtensions.SetLog"/> is called later on
        /// </summary>
        /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
        protected void AddToLogQueue(params object[] services) => DependencyLogs.Add(services);

        internal bool InitDone;

        void ILazyInitLog.SetLog(ILog parentLog)
        {
            if (InitDone) return;
            DependencyLogs.SetLog(parentLog);
            InitDone = true;
        }
    }

    public static class ServiceDependenciesExtensions
    {
        /// <summary>
        /// Auto-initialize the log on all dependencies.
        /// Special format to allow command chaining, so it returns itself.
        /// </summary>
        // TODO: @2dm - rename to ConnectServices
        public static TDependencies SetLog<TDependencies>(this TDependencies parent, ILog log) where TDependencies : ServiceDependencies
        {
            (parent as ILazyInitLog).SetLog(log);
            return parent;
        }

    }
}
