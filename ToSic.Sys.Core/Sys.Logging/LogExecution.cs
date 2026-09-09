using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>Execution-boundary context for the ILogger migration pilots.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogExecution
{
    public const string LogIdKey = "2sxc.LogId";
    public const string AmbientLogIdKey = "2sxc.AmbientLogId";

    /// <summary>
    /// Groups live events under an already admitted log and starts an optional Activity.
    /// Use once at a request/module boundary, with using across the complete awaited operation.
    /// This does not replace nested Fn/Done calls or create an Insights span tree.
    /// </summary>
    public static IDisposable? BeginExecution(this ILogger logger, ILog? log, ActivitySource source,
        string operation, int? siteId = null, int? pageId = null, int? moduleId = null, int? appId = null)
    {
        if (log.GetRealLog() is not Log root)
            return null;

        var properties = new Dictionary<string, object>
        {
            [LogIdKey] = root.LogId,
            [AmbientLogIdKey] = root.LogId,
            ["2sxc.Action"] = operation,
        };
        var activity = source.StartActivity(operation, ActivityKind.Internal);
        try
        {
            AddContext("SiteId", "2sxc.site.id", siteId);
            AddContext("PageId", "2sxc.page.id", pageId);
            AddContext("ModuleId", "2sxc.module.id", moduleId);
            AddContext("AppId", "2sxc.app.id", appId);
            return new ExecutionScope(logger.BeginScope(properties), activity);
        }
        catch
        {
            activity?.Dispose();
            throw;
        }

        void AddContext(string property, string tag, int? value)
        {
            if (!value.HasValue)
                return;
            properties["2sxc." + property] = value.Value;
            activity?.SetTag(tag, value.Value);
        }
    }

    private sealed class ExecutionScope(IDisposable? scope, Activity? activity) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                scope?.Dispose();
            }
            finally
            {
                activity?.Dispose();
            }
        }
    }
}
