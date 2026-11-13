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
    /// <returns></returns>
    public TService New(TOptions options)
    {
        var instance = sp.Build<TService>(Log);
        //_initCall?.Invoke(instance);
        return SetupOrThrow(options, instance);
    }

    public TService New(string key, TOptions options)
    {
        var instance = sp.Build<TService>(key, Log);
        return SetupOrThrow(options, instance);
    }

    public TService? TryNew(string key, TOptions options)
    {
        var instance = sp.TryBuild<TService>(key, Log);
        return instance == null
            ? instance
            : SetupOrThrow(options, instance);
    }

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

    private static TService SetupOrThrow(TOptions options, TService instance)
    {
        if (instance is not IServiceWithSetup<TOptions> settable)
            throw new NotSupportedException($"The service {typeof(TService).Name} doesn't support setting options of type {typeof(TOptions).Name}");
        settable.Setup(options);
        return instance;
    }

//    /// <summary>
//    /// Set the init-command as needed
//    /// </summary>
//    /// <param name="newInitCall"></param>
//    public Generator<TService, TOptions> SetInit(Action<TService> newInitCall)
//    {
//#if DEBUG
//        // Warn if we're accidentally replacing init-call, but only do this on debug
//        // In most cases it has no consequences, but we should write code that avoids this
//        if (_initCall != null)
//            throw new($"You tried to call {nameof(SetInit)} twice. This should never happen");
//#endif
//        _initCall = newInitCall;
//        return this;
//    }
//    private Action<TService>? _initCall;


}