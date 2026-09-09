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
    public static void SetSink(ILogEventSink? sink) => Volatile.Write(ref _sink, sink);

    internal static void Write(Entry entry, Exception? exception = null)
    {
        if (Volatile.Read(ref _sink) == null || _isWriting)
            return;
        Write(LogEvent.FromEntry(entry, exception), exception);
    }

    // The compatibility API already retains pre-admission entries. Replay those through ILogger
    // instead of maintaining a second provisional buffer. Insights never reads that buffer.
    internal static void Replay(Log log)
    {
        if (Volatile.Read(ref _sink) == null || _isWriting)
            return;
        foreach (var entry in log.SnapshotEntries().Where(e => !e.WrapClose))
            Write(LogEvent.FromEntry(entry) with { Replay = true });
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

    [ThreadStatic]
    private static bool _isWriting;
}
