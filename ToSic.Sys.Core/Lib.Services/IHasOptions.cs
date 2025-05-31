namespace ToSic.Lib.Services;

/// <summary>
/// Experimental; describes a service which has options, which must usually be set at setup.
/// </summary>
/// <typeparam name="TOptions">Options used to configure this service, usually a record.</typeparam>
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