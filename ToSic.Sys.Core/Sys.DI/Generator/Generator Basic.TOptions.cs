namespace ToSic.Sys.DI;

/// <summary>
/// Lazy generator to create multiple new services/objects of a specific type.
/// </summary>
/// <remarks>
/// Constructor should only be used in DI context and never be called directly.
/// </remarks>
/// <typeparam name="TService">The service to generate. It must implement <see cref="IServiceWithSetup{TOptions}"/></typeparam>
/// <typeparam name="TOptions">The options to configure the service</typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class Generator<TService, TOptions>(IServiceProvider sp) : IHasLog, ILazyInitLog
     where TService : IServiceWithSetup<TOptions> where TOptions : class?
{
    /// <summary>
    /// Factory method to generate a new service
    /// </summary>
    /// <param name="options">Options for setting up the service, usually non-null, or it will throw an error once it's accessed</param>
    /// <returns>The service, ready and initialized.</returns>
    public TService New(TOptions options)
    {
        var instance = sp.Build<TService>(Log);
        return SetupOrThrow(options, instance);
    }

    /// <summary>
    /// Retrieve a keyed service by key, with options.
    /// </summary>
    /// <param name="key">Key used in the registration - will throw if not found</param>
    /// <param name="options">Options for setting up the service, usually non-null, or it will throw an error once it's accessed</param>
    /// <returns>The service, ready and initialized.</returns>
    /// <exception cref="NotSupportedException">Throws if the provided options have the wrong type.</exception>
    public TService New(string key, TOptions options)
    {
        var instance = sp.Build<TService>(key, Log);
        return SetupOrThrow(options, instance);
    }

    /// <summary>
    /// Try to retrieve a keyed service by key, with options.
    /// </summary>
    /// <param name="key">Key used in the registration - will return null</param>
    /// <param name="options">Options for setting up the service, usually non-null, or it will throw an error once it's accessed</param>
    /// <returns>The service, ready and initialized - or null.</returns>
    /// <exception cref="NotSupportedException">Throws if the provided options have the wrong type.</exception>
    public TService? TryNew(string key, TOptions options)
    {
        var instance = sp.TryBuild<TService>(key, Log);
        return instance == null
            ? instance
            : SetupOrThrow(options, instance);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Throws if the provided options have the wrong type.</exception>
    /// <exception cref="NullReferenceException">Throws if the service is null.</exception>
    private static TService SetupOrThrow(TOptions options, TService instance)
    {
        if (instance is not IServiceWithSetup<TOptions> settable)
            throw new NotSupportedException($"The service {typeof(TService).Name} doesn't support setting options of type {typeof(TOptions).Name}");
        settable.Setup(options);
        return instance;
    }


    #region Logging

    /// <summary>
    /// Initializer to attach the log to the generator.
    /// The log is later given to generated objects.
    /// </summary>
    /// <param name="parentLog"></param>
    void ILazyInitLog.SetLog(ILog? parentLog)
        => Log = parentLog;

    /// <summary>
    /// The parent log, which is attached to newly generated objects
    /// _if_ they support logging.
    /// </summary>
    public ILog? Log { get; private set; }

    #endregion

}