using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

/// <summary>
/// Interface to mark classes which can dump their state into the log as a string.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface ICanDump
{
    /// <summary>
    /// Create a string dump of the current objects state/contents.
    /// </summary>
    /// <returns></returns>
    string Dump();
}