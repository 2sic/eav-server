using ToSic.Lib.Services;
using ToSic.Sys.Services;

namespace ToSic.Lib.DI;

/// <summary>
/// Lazy generator to create multiple new services/objects of a specific type.
/// </summary>
/// <remarks>
/// Constructor should only be used in DI context and never be called directly.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Generator<TService, TOptions>(IServiceProvider sp) : IHasLog, ILazyInitLog
     where TService : IHasOptions<TOptions> where TOptions : class
{
    /// <summary>
    /// Factory method to generate a new service
    /// </summary>
    /// <returns></returns>
    public TService New(TOptions options)
    {
        var instance = sp.Build<TService>(Log);
        _initCall?.Invoke(instance);

        if (instance is IServiceWithOptionsToSetup<TOptions> settable)
            settable.SetOptions(options);
        else
            throw new NotSupportedException($"The service {nameof(TOptions)} doesn't support setting options");
        return instance;
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

    /// <summary>
    /// Set the init-command as needed
    /// </summary>
    /// <param name="newInitCall"></param>
    public Generator<TService, TOptions> SetInit(Action<TService> newInitCall)
    {
#if DEBUG
        // Warn if we're accidentally replacing init-call, but only do this on debug
        // In most cases it has no consequences, but we should write code that avoids this
        if (_initCall != null)
            throw new($"You tried to call {nameof(SetInit)} twice. This should never happen");
#endif
        _initCall = newInitCall;
        return this;
    }
    private Action<TService>? _initCall;


}