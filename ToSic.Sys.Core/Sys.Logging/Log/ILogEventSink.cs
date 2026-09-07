using ToSic.Sys.Documentation;

namespace ToSic.Sys.Logging;

/// <summary>
/// Optional target for exporting 2sxc log entries to an external logging system.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILogEventSink
{
    /// <summary>
    /// Export an entry. Implementations must not write back to 2sxc logging.
    /// </summary>
    void Write(ILog log, Entry entry);
}
