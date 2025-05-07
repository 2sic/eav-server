namespace ToSic.Lib.Logging;

/// <summary>
/// Special interface to mark things which need to know when the log was connected.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ILogWasConnected
{
    /// <summary>
    /// Callback after connecting the log.
    /// </summary>
    void LogWasConnected();
}