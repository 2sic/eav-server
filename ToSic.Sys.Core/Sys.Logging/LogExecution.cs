using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>Execution-boundary context for the ILogger migration pilots.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogExecution
{
    public const string ActivitySourceName = MicrosoftLoggerEventSink.Category;
    public const string ActivitySourceVersion = ToSic.Sys.Assembly.SharedAssemblyInfo.AssemblyVersion;

    public const string LogIdKey = "2sxc.LogId";
    public const string AmbientLogIdKey = "2sxc.AmbientLogId";
    public const string AmbientOperationIdKey = "2sxc.AmbientOperationId";
    public const string InvocationLogIdKey = "2sxc.InvocationLogId";

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
        while (root.Parent is Log parent)
            root = parent;

        var properties = new Dictionary<string, object>
        {
            [LogIdKey] = root.LogId,
            [AmbientLogIdKey] = root.LogId,
            // A nested execution starts a new operation tree, even inside an invocation scope.
            [AmbientOperationIdKey] = 0L,
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

    /// <summary>
    /// Parents calls from an independent service logger to the invoking Fn/Done operation.
    /// Keep the scope around the complete awaited service call.
    /// </summary>
    public static IDisposable? BeginInvocation(this ILogger logger, ILogCall? parent)
    {
        if (parent?.Entry is not { Owner: { } owner } entry)
            return null;
        return logger.BeginScope(new Dictionary<string, object>
        {
            // Keep an enclosing execution's root. The owner is only a fallback outside a boundary.
            [InvocationLogIdKey] = owner.LogId,
            [AmbientOperationIdKey] = entry.Sequence,
        });
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
