using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>One semantic mapping for both DNN and Oqtane.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class MicrosoftLoggerEventSink(ILoggerFactory loggerFactory) : ILogEventSink
{
    public const string Category = "ToSic.2sxc";
    public const string StoreCategory = "ToSic.2sxc.Insights";
    private readonly ILogger _logger = loggerFactory.CreateLogger(Category);
    private readonly ILogger _storeLogger = loggerFactory.CreateLogger(StoreCategory);

    public void Write(LogEvent entry, Exception? exception = null)
    {
        var logger = entry.Replay || entry.Segment != null ? _storeLogger : _logger;
        logger.Log(entry.Level, new EventId(entry.WrapOpenWasClosed ? 2 : 1, "2sxc." + entry.Kind),
            entry, exception, static (state, _) => state.ToString());
    }
}
