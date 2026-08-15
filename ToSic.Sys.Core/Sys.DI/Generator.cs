namespace ToSic.Sys.DI;

/// <summary>
/// Lazy generator to create multiple new services/objects of a specific type.
/// </summary>
/// <remarks>
/// Constructor should only be used in DI context and never be called directly.
/// </remarks>
/// <typeparam name="TService">The service to generate. It must implement <see cref="IServiceWithSetup{TOptions}"/></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class Generator<TService>(IServiceProvider sp) : IHasLog, ILazyInitLog
{
    #region Logging

    /// <inheritdoc/>
    void ILazyInitLog.SetLog(ILog? parentLog)
        => Log = parentLog;

    /// <inheritdoc cref="LazySvc{TService}.Log"/>
    public ILog? Log { get; private set; }

    #endregion

    
    #region Get / New / Value

    /// <summary>
    /// Factory method to generate a new service
    /// </summary>
    /// <returns></returns>
    public TService New()
    {
        var service = sp.Build<TService>(Log);
        _initCall?.Invoke(service);
        return service;
    }

    /// <summary>
    /// Factory method to generate a new service using a name/keyed.
    /// </summary>
    /// <param name="key">The key/name of the service to generate.</param>
    /// <remarks>
    /// New in v22
    /// </remarks>
    /// <returns></returns>
    public TService New(string key)
    {
        var service = sp.Build<TService>(key, Log);
        _initCall?.Invoke(service);
        return service;
    }
    
    #endregion


    #region SetInit

    /// <summary>
    /// Set the init-command as needed
    /// </summary>
    /// <param name="newInitCall"></param>
    /// <param name="allowReplace">Allow replacing the set-init</param>
    public Generator<TService> SetInit(Action<TService> newInitCall, bool allowReplace = false)
    {
        _initCall = LazyHelpers.ThrowIfInitAlreadySet(_initCall, newInitCall, allowReplace);
        return this;
    }
    private Action<TService>? _initCall;

    #endregion

}