using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services;

internal class DependencyLogs
{
    /// <summary>
    /// Collects all objects which support `SetLog(Log)` for LazyInitLogs
    /// </summary>
    internal List<ILazyInitLog> LazyInitLogs { get; } = [];

    /// <summary>
    /// Collects all objects which support `Init(Log)`
    /// </summary>
    internal List<IHasLog> HasLogs { get; } = [];

    /// <summary>
    /// Add objects to various queues to be auto-initialized when <see cref="SetLog"/> is called later on
    /// </summary>
    /// <param name="services"></param>
    public void Add(object[] services)
    {
        var lazyInitLogs = services
            .Where(s => s is ILazyInitLog)
            .Cast<ILazyInitLog>();
        LazyInitLogs.AddRange(lazyInitLogs);

        var hasLog = services
            .Where(s => s is not ILazyInitLog && s is IHasLog)
            .Cast<IHasLog>();
        HasLogs.AddRange(hasLog);

        // Temporary warning to detect if IServiceProvider is being passed in
        if (services.Any(s => s is IServiceProvider))
            throw new Exception("IServiceProvider should not be passed in as a dependency.");
    }

    /// <summary>
    /// Auto-initialize the log on all dependencies
    /// </summary>
    public void SetLog(ILog log)
    {
        LazyInitLogs.ForEach(s => s.SetLog(log));
        HasLogs.ForEach(hl => hl.LinkLog(log));
    }
}