﻿namespace ToSic.Sys.Logging;

/// <summary>
/// Interface to mark classes which can dump their state into the log as a string.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanDump
{
    /// <summary>
    /// Create a string dump of the current objects state/contents.
    /// </summary>
    /// <returns></returns>
    string Dump();
}