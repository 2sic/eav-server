﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DI
{
    public class DependencyLogs
    {
        /// <summary>
        /// Collects all objects which support `SetLog(Log)` for LazyInitLogs
        /// </summary>
        internal List<ILazyInitLog> LazyInitLogs { get; } = new List<ILazyInitLog>();

        /// <summary>
        /// Collects all objects which support `Init(Log)`
        /// </summary>
        internal List<IHasLog> HasLogs { get; } = new List<IHasLog>();

        /// <summary>
        /// Add objects to various queues to be auto-initialized when <see cref="SetLog"/> is called later on
        /// </summary>
        /// <param name="services"></param>
        public void Add(params object[] services)
        {
            var lazyInitLogs = services.Where(s => s is ILazyInitLog).Cast<ILazyInitLog>();
            LazyInitLogs.AddRange(lazyInitLogs);
            var hasLog = services.Where(s => !(s is ILazyInitLog) && s is IHasLog).Cast<IHasLog>();
            HasLogs.AddRange(hasLog);
        }

        /// <summary>
        /// Auto-initialize the log on all dependencies
        /// </summary>
        public void SetLog(ILog log)
        {
            LazyInitLogs.ForEach(s => s.SetLog(log));
            HasLogs.ForEach(hl => hl.Init(log));
            //return this as TDependencies;
        }
    }
}
