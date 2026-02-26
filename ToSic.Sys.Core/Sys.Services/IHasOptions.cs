namespace ToSic.Sys.Services;

/// <summary>
/// Experimental; describes a service which has options, which must usually be set at setup.
/// </summary>
/// <typeparam name="TOptions">Options used to configure this service, usually a record.</typeparam>
[WorkInProgressApi("Still bound to change a bit")]
public interface IHasOptions<out TOptions>
    where TOptions : class
{
    /// <summary>
    /// The options instance for this service, read-only.
    /// </summary>
    /// <remarks>
    /// 1. Explicitly with a short name "MyOptions" so that the classes can have a subclass called "Options" without name conflicts.
    /// 2. Will usually default to new/standard options of type <typeparamref name="TOptions"/>.
    /// </remarks>
    TOptions MyOptions { get; }

}