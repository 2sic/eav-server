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
/// <typeparam name="TOptions"></typeparam>
public interface IHasOptions<out TOptions>
    where TOptions : class
{
    /// <summary>
    /// The options for this service, read-only.
    /// </summary>
    /// <remarks>
    /// Will usually default to new/standard options of type <see cref="TOptions"/>,
    /// </remarks>
    TOptions Options { get; }

}