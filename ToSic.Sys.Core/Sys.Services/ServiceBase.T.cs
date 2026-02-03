namespace ToSic.Sys.Services;

/// <summary>
/// Base class for any service which expects a Dependencies class
/// </summary>
/// <typeparam name="TDependencies"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
// #NoEditorBrowsableBecauseOfInheritance
//[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ServiceBase<TDependencies>: ServiceBase where TDependencies : IDependencies
{
    /// <summary>
    /// Constructor for normal case, with services
    /// </summary>
    /// <param name="services">Dependencies to auto-attach to property `Services`</param>
    /// <param name="logName">The new objects name in the logs</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="connect">Optional array of services to connect the logs to.</param>
    protected ServiceBase(TDependencies services, string logName, NoParamOrder npo = default, object[]? connect = default)
        : base(logName, connect: connect)
    {
        Services = services.ConnectServices(Log);
    }

    /// <summary>
    /// The services which came through the `TDependencies services` in the constructor.
    /// </summary>
    protected readonly TDependencies Services;
}