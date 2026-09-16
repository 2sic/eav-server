using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>Execution-boundary context for the ILogger migration pilots.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogExecution
{
    private static readonly AsyncLocal<ExecutionFrame?> CurrentFrame = new();

    internal static string? CurrentExecutionId
    {
        get
        {
            var frame = CurrentFrame.Value;
            while (frame is { Active: false })
                frame = frame.Previous;
            return frame?.ExecutionId;
        }
    }

    /// <summary>True while the current async flow is already owned by an admitted execution.</summary>
    public static bool HasActiveExecution => CurrentExecutionId != null;

    public const string ActivitySourceName = MicrosoftLoggerEventSink.Category;
    public const string ActivitySourceVersion = ToSic.Sys.Assembly.SharedAssemblyInfo.AssemblyVersion;

    public const string LogIdKey = "2sxc.LogId";
    public const string AmbientLogIdKey = "2sxc.AmbientLogId";
    public const string AmbientOperationIdKey = "2sxc.AmbientOperationId";
    public const string InvocationLogIdKey = "2sxc.InvocationLogId";
    public const string SourceLogIdKey = "2sxc.SourceLogId";

    /// <summary>
    /// Groups live events under an already admitted log and starts an optional Activity.
    /// Use once at a request/module boundary, with using across the complete awaited operation.
    /// This does not replace nested Fn/Done calls or create an Insights span tree.
    /// </summary>
    public static IDisposable? BeginExecution(this ILogger logger, ILog? log, ActivitySource source,
        string operation, int? siteId = null, int? pageId = null, int? moduleId = null, int? appId = null)
        => BeginExecution(logger, log, (log.GetRealLog() as Log)?.LatestExecutionId, source, operation,
            siteId, pageId, moduleId, appId);

    /// <summary>Starts an execution using the exact admission returned by the log store.</summary>
    public static IDisposable? BeginExecution(this ILogger logger, LogStoreEntry? entry, ActivitySource source,
        string operation, int? siteId = null, int? pageId = null, int? moduleId = null, int? appId = null)
        => BeginExecution(logger, entry?.Log,
            LogEventBridge.UsesExecutionContext ? entry?.ExecutionId : null, source, operation,
            siteId, pageId, moduleId, appId);

    private static IDisposable? BeginExecution(ILogger logger, ILog? log, string? executionId, ActivitySource source,
        string operation, int? siteId, int? pageId, int? moduleId, int? appId)
    {
        if (log.GetRealLog() is not Log root)
            return null;
        while (!LogEventBridge.UsesExecutionContext && root.Parent is Log parent)
            root = parent;
        executionId ??= root.LogId;

        var properties = new Dictionary<string, object>
        {
            [LogIdKey] = executionId,
            [AmbientLogIdKey] = executionId,
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
            var frame = new ExecutionFrame(executionId, CurrentFrame.Value);
            CurrentFrame.Value = frame;
            try
            {
                return new ExecutionScope(logger.BeginScope(properties), activity, frame);
            }
            catch
            {
                frame.Close();
                if (ReferenceEquals(CurrentFrame.Value, frame))
                    CurrentFrame.Value = frame.Previous;
                throw;
            }
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
            // The entry captured its execution when the call started; the producer may since have been re-admitted.
            [InvocationLogIdKey] = entry.ExecutionId ?? owner.LogId,
            [AmbientOperationIdKey] = entry.Sequence,
        });
    }

    private sealed class ExecutionScope(IDisposable? scope, Activity? activity, ExecutionFrame frame) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                try
                {
                    scope?.Dispose();
                }
                catch
                {
                    // A logging provider's cleanup failure must not replace an application exception.
                }
            }
            finally
            {
                try
                {
                    activity?.Dispose();
                }
                finally
                {
                    frame.Close();
                    if (ReferenceEquals(CurrentFrame.Value, frame))
                        CurrentFrame.Value = frame.Previous;
                }
            }
        }
    }

    private sealed class ExecutionFrame(string executionId, ExecutionFrame? previous)
    {
        private int _active = 1;
        internal string ExecutionId { get; } = executionId;
        internal ExecutionFrame? Previous { get; } = previous;
        internal bool Active => Volatile.Read(ref _active) != 0;
        internal void Close() => Interlocked.Exchange(ref _active, 0);
    }
}

internal static class LogOperationContext
{
    private static readonly AsyncLocal<Frame?> CurrentFrame = new();

    internal static Frame? Current
    {
        get
        {
            var executionId = LogExecution.CurrentExecutionId;
            var frame = CurrentFrame.Value;
            while (frame != null && (!frame.Active || executionId != null && frame.ExecutionId != executionId))
                frame = frame.Previous;
            return frame;
        }
    }

    internal static IDisposable Begin(Entry entry)
    {
        var frame = new Frame(entry, CurrentFrame.Value);
        CurrentFrame.Value = frame;
        return new Scope(frame);
    }

    internal sealed class Frame(Entry entry, Frame? previous)
    {
        private int _active = 1;
        internal Entry Entry { get; } = entry;
        internal string ExecutionId { get; } = entry.ExecutionId ?? entry.Owner!.LogId;
        internal Frame? Previous { get; } = previous;
        internal bool Active => Volatile.Read(ref _active) != 0;
        internal int Depth
        {
            get
            {
                var depth = 0;
                for (var frame = Previous; frame != null; frame = frame.Previous)
                    if (frame.Active && frame.ExecutionId == ExecutionId)
                        depth++;
                return depth;
            }
        }
        internal void Close() => Interlocked.Exchange(ref _active, 0);
    }

    private sealed class Scope(Frame frame) : IDisposable
    {
        public void Dispose()
        {
            frame.Close();
            if (ReferenceEquals(CurrentFrame.Value, frame))
                CurrentFrame.Value = frame.Previous;
        }
    }
}
