namespace ToSic.Lib.Logging;

/// <summary>
/// Interface to mark special objects which have a log,
/// but should never be connected to anything else.
///
/// This is especially important for long-running objects such as a cache, which if connected could cause
/// quite a bit of trouble.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ILogShouldNeverConnect
{
}