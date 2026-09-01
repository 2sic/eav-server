namespace ToSic.Sys.DI;

/// <summary>
/// Helps us create lazy **Service** objects.
/// </summary>
/// <typeparam name="TService">Service type, ideally based on <see cref="ServiceBase"/></typeparam>
/// <param name="sp">Service provider, in case we need to debug something</param>
/// <remarks>
/// It has some special features to reduce the amount of plumbing in code:
/// 
/// * It will automatically lazy-attach a logger when used correctly
/// * It can also be configured with a lazy init function to keep code clean.
///
/// LazySvc will detect if the provided object/service supports these features.
/// So if it's used for anything that doesn't support logging it will just behave like `Lazy`.
///
/// Notes
/// 
/// * Constructor should never be called as it's only meant to be used with Dependency Injection.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class LazySvc<TService>(IServiceProvider sp)
    : ILazyLike<TService>, IHasLog, ILazyInitLog
    where TService : class
{
    #region Logging

    /// <inheritdoc/>
    void ILazyInitLog.SetLog(ILog? parentLog)
        => Log = parentLog;

    /// <summary>
    /// The parent log, which is attached to newly generated objects
    /// _if_ they support logging.
    /// </summary>
    public ILog? Log { get; private set; }

    #endregion

    
    #region Get / New / Value

    public TService Value => _valueGet.Get(() =>
    {
        var value = sp.Build<TService>(Log);
        _initCall?.Invoke(value);
        return value;
    })!;
    private readonly LazyGetAndReset<TService> _valueGet = new();

    public bool IsValueCreated => _valueGet.IsValueCreated;

    #endregion


    #region SetInit

    /// <summary>
    /// Set the init-command as needed
    /// </summary>
    /// <param name="newInitCall"></param>
    /// <param name="allowReplace">Allow replacing the set-init</param>
    public LazySvc<TService> SetInit(Action<TService> newInitCall, bool allowReplace = false)
    {
        _initCall = LazyHelpers.ThrowIfInitAlreadySet(_initCall, newInitCall, allowReplace);
        return this;
    }
    private Action<TService>? _initCall;

    #endregion

    /// <summary>
    /// EXPERIMENTAL - replace a service with an already prepared one, to bypass the default factory in edge cases
    /// </summary>
    /// <param name="replacement"></param>
    public void Inject(TService replacement)
        => _valueGet.Set(replacement);
}