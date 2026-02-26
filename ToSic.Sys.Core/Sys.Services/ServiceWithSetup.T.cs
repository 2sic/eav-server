namespace ToSic.Sys.Services;

// #NoEditorBrowsableBecauseOfInheritance
//[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ServiceWithSetup<TServices, TOptions> : ServiceBase,
    IHasOptions<TOptions>,
    IServiceWithSetup<TOptions>
    where TServices : IDependencies
    where TOptions : class, new()
{
    /// <summary>
    /// Constructor for normal case, with services
    /// </summary>
    /// <param name="services">Dependencies to auto-attach to property `Services`</param>
    /// <param name="logName">The new objects name in the logs</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="connect">Optional array of services to connect the logs to.</param>
    protected ServiceWithSetup(TServices services, string logName, NoParamOrder npo = default, object[]? connect = default)
        : base(logName, connect: connect)
    {
        Services = services.ConnectServices(Log);
    }

    /// <summary>
    /// The services which came through the `TDependencies services` in the constructor.
    /// </summary>
    protected readonly TServices Services;

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions MyOptions
    {
        get => field ??= GetDefaultOptions();
        private set;
    }

    public virtual void Setup(TOptions options)
        => MyOptions = options;

    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions()
        => new();
}