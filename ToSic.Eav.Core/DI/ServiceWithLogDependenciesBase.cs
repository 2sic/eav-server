using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DI
{
    public abstract class ServiceWithLogDependenciesBase: HasLog
    {
        [PrivateApi]
        protected ServiceWithLogDependenciesBase(string logName) : base(logName)
        {

        }

        /// <summary>
        /// Special helper to keep track of all dependencies which need a log, to init once SetLog is called
        /// </summary>
        private DependencyLogs DependencyLogs { get; } = new DependencyLogs();

        /// <summary>
        /// Add Log to all dependencies listed in <see cref="services"/>
        /// </summary>
        /// <param name="log">The current log object</param>
        /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
        [PrivateApi]
        protected void InitServicesLogs(ILog log, params object[] services)
        {
            DependencyLogs.Add(services);
            DependencyLogs.SetLog(log);
        }
    }
}
