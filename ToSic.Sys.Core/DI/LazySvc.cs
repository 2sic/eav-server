using ToSic.Lib.Helpers;

namespace ToSic.Lib.DI;

/// <summary>
/// Helps us create lazy **Service** objects. It has some special features:
/// 
/// * It will automatically lazy-attach a logger when used correctly
/// * It can also be configured with a lazy init function to keep code clean.
/// 
/// This reduces the amount of plumbing in many code files.
///
/// It will detect if the provided object/service supports these features.
/// So if it's used for anything that doesn't support logging it will just behave like `Lazy`.
/// </summary>
/// <typeparam name="TService">Service type, ideally based on <see cref="ToSic.Lib.Services.ServiceBase"/></typeparam>
/// <remarks>
/// Constructor, should never be called as it's only meant to be used with Dependency Injection.
/// </remarks>
/// <param name="sp">
/// Service provider, in case we need to debug something
/// </param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LazySvc<TService>(IServiceProvider sp)
    : ILazyLike<TService>, IHasLog, ILazyInitLog
    where TService : class
{
    /// <summary>
    /// Set the init-command as needed
    /// </summary>
    /// <param name="newInitCall"></param>
    public LazySvc<TService> SetInit(Action<TService> newInitCall)
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

    public TService Value => _valueGet.Get(() =>
    {
        var value = sp.Build<TService>(Log);
        _initCall?.Invoke(value);
        return value;
    })!;
    private readonly GetOnce<TService> _valueGet = new();

    public bool IsValueCreated => _valueGet.IsValueCreated;

    /// <summary>
    /// EXPERIMENTAL - replace a service with an already prepared one, to bypass the default factory in edge cases
    /// </summary>
    /// <param name="replacement"></param>
    public void Inject(TService replacement)
        => _valueGet.Reset(replacement);

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

}