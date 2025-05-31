namespace ToSic.Lib.Services;

/// <summary>
/// Experimental; describes a service which can be configured, but will generate a new instance for it.
/// </summary>
/// <remarks>
/// WIP
/// The idea is that certain services can be configured, but that they must be immutable.
/// Since we can't configure them at construction, we must somehow create a new instance, and set the options.
/// To make this work:
///
/// 1. There must be an `Options` which are public get, auto-default so something useful, and private set.
/// 2. There must be a `New(options)` method which returns a new instance of the service.
/// </remarks>
/// <typeparam name="TService"></typeparam>
/// <typeparam name="TOptions"></typeparam>
public interface IServiceRespawn<out TService, in TOptions>
    // : IServiceWithSetup<TOptions>
        // where TService : class, IServiceRespawn<TService, TOptions>
        // where TOptions : class
{
    /// <summary>
    /// Generate a new instance of the service, using alternate options.
    /// </summary>
    /// <param name="options">The options</param>
    /// <returns></returns>
    public TService SpawnNew(TOptions options);
}