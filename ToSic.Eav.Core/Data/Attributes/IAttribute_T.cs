namespace ToSic.Eav.Data;

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

    // 2024-06-04 2dm disabled completely, cannot imagine anybody using this on the Typed-Interface - clean-up 2024-Q3
    ///// <summary>
    ///// Gets the Value for the specified Language/Dimension using the ID accessor. Usually not needed. Typed.
    ///// </summary>
    ///// <param name="languageId">the language id (number)</param>
    //new T this[int languageId] { get; }

}