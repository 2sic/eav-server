﻿using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DI
{
    /// <summary>
    /// Special base class for most services which have a bunch of child-services that should be log-connected.
    ///
    /// Provides a special Dependency-list and will auto-set the log for all if they support logs.
    /// </summary>
    [PrivateApi]
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
        /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
        [PrivateApi]
        protected void ConnectServices(params object[] services)
        {
            DependencyLogs.Add(services);
            DependencyLogs.SetLog(Log);
        }
    }
}
