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
/// * For testing, use the `LazySvcInjectable` in the TestHelpers project, which allows you to provide a pre-created object.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class LazySvc<TService>(IServiceProvider sp)
    : ILazyLike<TService>, IHasLog, ILazyInitLog
    where TService : class
{
    #region Get / New / Value

    public virtual TService Value => _valueGet.Get(() => sp.Build<TService>(Log))!;
    private readonly LazyGet<TService> _valueGet = new();

    public bool IsValueCreated => _valueGet.IsValueCreated;

    #endregion



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
}
