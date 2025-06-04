namespace ToSic.Eav.Data.Attributes.Sys;

/// <summary>
/// Represents an Attribute (Property), but strongly typed
/// </summary>
/// <remarks>
/// > We recommend you read about the [](xref:Basics.Data.Index)
/// </remarks>
/// <typeparam name="T">Type of the Value</typeparam>
[PublicApi]
public interface IAttribute<out T> : IAttribute
{
    /// <summary>
    /// Gets the typed first/default value
    /// </summary>
    T TypedContents { get; }

    /// <summary>
    /// Gets the typed Value Objects - so the same as Values, but with the correct type
    /// </summary>
    IEnumerable<IValue<T>> Typed { get; }
}