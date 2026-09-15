using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>One semantic mapping for both DNN and Oqtane.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class MicrosoftLoggerEventSink(ILoggerFactory loggerFactory, bool forwardExternally = true) : ILogEventSink
{
    public const string Category = "ToSic.2sxc";
    public const string StoreCategory = "ToSic.2sxc.Insights";
    private readonly ILogger _logger = loggerFactory.CreateLogger(Category);
    private readonly ILogger _storeLogger = loggerFactory.CreateLogger(StoreCategory);

    public bool IsEnabled(LogLevel logLevel)
        => _storeLogger.IsEnabled(logLevel) || forwardExternally && _logger.IsEnabled(logLevel);

    public void Write(LogEvent entry, Exception? exception = null)
    {
        var eventId = new EventId(entry.WrapOpenWasClosed ? 2 : 1, "2sxc." + entry.Kind);
        var logger = forwardExternally && !entry.Replay && entry.Segment == null ? _logger : _storeLogger;
        logger.Log(entry.Level, eventId, entry, exception, static (state, _) => state.ToString());
    }
}
