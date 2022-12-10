using ToSic.Lib.Logging;

namespace ToSic.Eav.DI
{
    /// <summary>
    /// Experimental: Base class for all dependencies helpers.
    ///
    /// Can collect all objects which need the log and init that.
    /// </summary>
    /// <typeparam name="TDependencies"></typeparam>
    public class DependenciesBase<TDependencies> where TDependencies : DependenciesBase<TDependencies>
    {
        private DependencyLogs DependencyLogs { get; } = new DependencyLogs();

        /// <summary>
        /// Add objects to various queues to be auto-initialized when <see cref="SetLog"/> is called later on
        /// </summary>
        /// <param name="services"></param>
        protected void AddToLogQueue(params object[] services) => DependencyLogs.Add(services);

        /// <summary>
        /// Auto-initialize the log on all dependencies.
        /// Special format to allow command chaining, so it returns itself.
        /// </summary>
        public virtual TDependencies SetLog(ILog log)
        {
            DependencyLogs.SetLog(log);
            return this as TDependencies;
        }
    }
}
