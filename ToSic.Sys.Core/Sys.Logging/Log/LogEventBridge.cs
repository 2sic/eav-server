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

    internal static void Write(ILog log, Entry entry)
    {
        var sink = Volatile.Read(ref _sink);
        if (sink == null || _isWriting)
            return;

        try
        {
            _isWriting = true;
            sink.Write(log, entry);
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
