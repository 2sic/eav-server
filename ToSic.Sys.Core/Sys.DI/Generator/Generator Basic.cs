namespace ToSic.Sys.DI;

/// <summary>
/// Generator to create multiple new services/objects of a specific type.
/// </summary>
/// <remarks>
/// Constructor should only be used in DI context and never be called directly.
/// </remarks>
/// <typeparam name="TService">The service to generate. It must implement <see cref="IServiceWithSetup{TOptions}"/></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class GeneratorBasic<TService>(IServiceProvider sp) : IHasLog, ILazyInitLog
    where TService: class
{
    #region Get / New / Value

    /// <summary>
    /// Factory method to generate a new service
    /// </summary>
    /// <returns></returns>
    public TService New() => sp.Build<TService>(Log);

    /// <summary>
    /// Factory method to generate a new service using a name/keyed.
    /// </summary>
    /// <param name="key">The key/name of the service to generate.</param>
    /// <remarks>
    /// New in v22
    /// </remarks>
    /// <returns></returns>
    public TService New(string key) => sp.Build<TService>(key, Log);

    /// <summary>
    /// Factory method to generate a new service using a name/keyed.
    /// </summary>
    /// <param name="key">The key/name of the service to generate.</param>
    /// <remarks>
    /// New in v22
    /// </remarks>
    /// <returns></returns>
    public TService? TryNew(string key) => sp.TryBuild<TService>(key, Log);

    #endregion



    #region Logging

    /// <inheritdoc/>
    void ILazyInitLog.SetLog(ILog? parentLog)
        => Log = parentLog;

    /// <inheritdoc cref="LazySvc{TService}.Log"/>
    public ILog? Log { get; private set; }

    #endregion
}
