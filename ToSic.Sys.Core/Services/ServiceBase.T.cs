using ToSic.Lib.Coding;

namespace ToSic.Lib.Services;

/// <summary>
/// Base class for any service which expects a Dependencies class
/// </summary>
/// <typeparam name="TMyServices"></typeparam>
// #NoEditorBrowsableBecauseOfInheritance
//[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ServiceBase<TMyServices>: ServiceBase where TMyServices : MyServicesBase
{
    /// <summary>
    /// Constructor for normal case, with services
    /// </summary>
    /// <param name="services">Services to auto-attach to property `Services`</param>
    /// <param name="logName">The new objects name in the logs</param>
    /// <param name="protect"></param>
    /// <param name="connect"></param>
    protected ServiceBase(TMyServices services, string logName, NoParamOrder protect = default, object[]? connect = default) : base(logName, connect: connect)
    {
        Services = services.ConnectServices(Log);
    }

    /// <summary>
    /// Constructor for passing on service dependencies which are extended by a inheriting dependencies.
    /// </summary>
    /// <param name="extendedServices"></param>
    /// <param name="logName"></param>
    /// <param name="protect"></param>
    /// <param name="connect"></param>
    protected ServiceBase(MyServicesBase<TMyServices> extendedServices, string logName, NoParamOrder protect = default, object[]? connect = default)
        : this(extendedServices.ParentServices, logName, connect: connect)
    {
        // Ensure the extended copy also has SetLog run
        extendedServices.ConnectServices(Log);
    }

    protected readonly TMyServices Services;
}