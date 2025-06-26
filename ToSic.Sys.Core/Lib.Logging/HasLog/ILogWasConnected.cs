namespace ToSic.Lib.Logging;

/// <summary>
/// Special interface to mark things which need to know when the log was connected.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILogWasConnected
{
    /// <summary>
    /// Callback after connecting the log.
    /// </summary>
    void LogWasConnected();
}