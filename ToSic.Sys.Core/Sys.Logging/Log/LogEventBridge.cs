namespace ToSic.Sys.Logging;

/// <summary>
/// Connects 2sxc logging to one optional external event sink.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogEventBridge
{
    /// <summary>
    /// Configuration key used by host integrations to enable the bridge.
    /// </summary>
    public const string EnabledConfigurationKey = "Logging:2sxc:Enabled";

    /// <summary>
    /// Set or clear the process-wide sink used by new log entries.
    /// </summary>
    public static void SetSink(ILogEventSink? sink)
    {
        Volatile.Write(ref _sink, sink);
        if (sink == null)
            SetMode(LogStoreMode.Legacy);
    }

    internal static bool UsesExecutionContext => Volatile.Read(ref _mode) == (int)LogStoreMode.ILogger;

    internal static void SetMode(LogStoreMode mode) => Volatile.Write(ref _mode, (int)mode);

    internal static IDisposable? BeginOperation(Entry? entry)
    {
        if (!UsesExecutionContext || entry is not { Owner: { } })
            return null;
        try
        {
            return LogOperationContext.Begin(entry);
        }
        catch
        {
            return null;
        }
    }

    internal static void Write(Entry entry, Exception? exception = null)
    {
        if (Volatile.Read(ref _sink) != null)
            entry.BridgePublicationAttempted = true;
        if (!IsEnabled(entry.Level))
            return;
        Write(LogEvent.FromEntry(entry), exception);
    }

    // The compatibility API already retains pre-admission entries. Replay those through ILogger
    // instead of maintaining a second provisional buffer. Insights never reads that buffer.
    internal static void Replay(Log log)
    {
        if (Volatile.Read(ref _sink) == null || _isWriting)
            return;
        foreach (var entry in log.SnapshotEntries().Where(e => !e.WrapClose && (!UsesExecutionContext || !e.BridgePublicationAttempted)))
        {
            entry.BridgePublicationAttempted = true;
            if (!IsEnabled(entry.Level))
                continue;
            var replay = LogEvent.FromEntry(entry) with { Replay = true };
            if (UsesExecutionContext && replay.Ancestors.Length == 0 && log.LatestExecutionId is { } executionId)
                replay = replay with { Ancestors = [executionId] };
            Write(replay);
        }
    }

    private static bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        var sink = Volatile.Read(ref _sink);
        if (sink == null || _isWriting)
            return false;
        try
        {
            _isWriting = true;
            return sink.IsEnabled(logLevel);
        }
        catch
        {
            // External logging must never break the authoritative 2sxc logging path.
            return false;
        }
        finally
        {
            _isWriting = false;
        }
    }

    internal static void Write(LogEvent entry, Exception? exception = null)
    {
        var sink = Volatile.Read(ref _sink);
        if (sink == null || _isWriting)
            return;

        try
        {
            _isWriting = true;
            sink.Write(entry, exception);
        }
        catch
        {
            // External logging must never break the authoritative 2sxc logging path.
        }
        finally
        {
            _isWriting = false;
        }
    }

    private static ILogEventSink? _sink;
    private static int _mode;

    [ThreadStatic]
    private static bool _isWriting;
}
