﻿using ToSic.Lib.DI;

namespace ToSic.Lib.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class HasLog_ConnectServices
{
    /// <summary>
    /// Add Log to all dependencies listed in <see cref="services"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="services">One or more services which could implement <see cref="LazySvc{T}"/> or <see cref="IHasLog"/></param>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static void ConnectLogs(this IHasLog parent, object[] services)
    {
        var depLogs = new DependencyLogs();
        depLogs.Add(services);
        depLogs.SetLog(parent.Log);
    }
}