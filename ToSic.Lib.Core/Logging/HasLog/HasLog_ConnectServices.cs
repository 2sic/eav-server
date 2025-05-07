using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Lib.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class HasLog_ConnectServices
{
    /// <summary>
    /// Add Log to all dependencies listed in <see cref="services"/>
    /// </summary>
    /// <param name="services">One or more services which could implement <see cref="LazySvc{T}"/> or <see cref="IHasLog"/></param>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void ConnectLogs(this IHasLog parent, object[] services)
    {
        var depLogs = new DependencyLogs();
        depLogs.Add(services);
        depLogs.SetLog(parent.Log);
    }
}