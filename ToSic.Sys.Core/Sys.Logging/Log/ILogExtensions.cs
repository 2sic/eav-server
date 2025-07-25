﻿namespace ToSic.Sys.Logging;

/// <summary>
/// Various extensions for <see cref="ILog"/> objects to add logs.
/// They are all implemented as extension methods, so that they will not fail even if the log object is null.
/// </summary>
[PublicApi]
// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static partial class ILogExtensions
{
    /// <summary>
    /// Special call to get a sub-log object or null.
    /// Null is what we get if the parent was null, because in that case we shouldn't be logging in the first place.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="name"></param>
    /// <param name="enabled"></param>
    /// <returns></returns>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static ILog? SubLogOrNull(this ILog? log, string name, bool enabled = true)
    {
        if (!enabled)
            return null;
        var realLog = log.GetRealLog();
        return realLog == default
            ? null
            : new Log(name, realLog);
    }
        
}