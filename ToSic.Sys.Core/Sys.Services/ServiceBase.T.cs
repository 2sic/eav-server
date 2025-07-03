namespace ToSic.Sys.Services;

/// <summary>
/// Base class for any service which expects a Dependencies class
/// </summary>
/// <typeparam name="TMyServices"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
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
    protected ServiceBase(TMyServices services, string logName, NoParamOrder protect = default, object[]? connect = default)
        : base(logName, connect: connect)
    {
        Services = services.ConnectServices(Log);
    }

    protected readonly TMyServices Services;
}