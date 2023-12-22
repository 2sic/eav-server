using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services;

/// <summary>
/// Base class for all MyServices.
/// These are helper objects to get dependencies for a class.
/// It should be used when the owning-class is expected to be inherited.
/// This is important for _inheriting_ classes to keep a stable constructor.
///
/// Can collect all objects which need the log and init that.
/// </summary>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class MyServicesBase: ILazyInitLog
{
    /// <summary>
    /// Special helper to keep track of all dependencies which need a log, to init once SetLog is called
    /// </summary>
    internal DependencyLogs DependencyLogs { get; } = new();

    /// <summary>
    /// Add objects to various queues to be auto-initialized when <see cref="ServiceDependenciesExtensions.ConnectServices{TDependencies}"/> is called later on
    /// </summary>
    /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
    protected void ConnectServices(params object[] services) => DependencyLogs.Add(services);

    internal bool InitDone;

    void ILazyInitLog.SetLog(ILog parentLog)
    {
        if (InitDone) return;
        DependencyLogs.SetLog(parentLog);
        InitDone = true;
    }
}

[PrivateApi]
public static class ServiceDependenciesExtensions
{
    /// <summary>
    /// Auto-initialize the log on all dependencies.
    /// Special format to allow command chaining, so it returns itself.
    /// </summary>
    // TODO: @2dm - rename to ConnectServices
    public static TDependencies ConnectServices<TDependencies>(this TDependencies parent, ILog log) where TDependencies : MyServicesBase
    {
        (parent as ILazyInitLog).SetLog(log);
        return parent;
    }

}