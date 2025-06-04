using System.Collections.Immutable;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents an Attribute with Values - without knowing what data type is in the value.
/// Usually we'll extend this and use <see cref="IAttribute{T}"/> instead.
/// </summary>
/// <remarks>
/// > We recommend you read about the [](xref:Basics.Data.Index)
/// </remarks>
[PublicApi]
public interface IAttribute : IAttributeBase
{
    /// <summary>
    /// Gets a list of all <see cref="IValue"/>s of this Entity's Attribute. To get the typed objects, use the <see cref="IAttribute{T}.Typed"/>
    /// </summary>
    IEnumerable<IValue> Values { get; } 

    #region get-value eaccessors

    /// <summary>
    /// Gets the Value for the specified Language/Dimension using the ID accessor. Usually not needed. Untyped.
    /// </summary>
    /// <param name="languageId">the language id (number)</param>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    object this[int languageId] { get; }

    /// <summary>
    /// Get the best/first matching value for the specified language key - untyped
    /// </summary>
    /// <param name="languageKey">The language key (string) to look for</param>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    object this[string languageKey] { get; }


    #endregion


    /// <summary>
    /// Get the value and internal object result.
    /// Note that the language keys MUST be lower-cased and everything.
    /// </summary>
    /// <param name="languageKeys"></param>
    /// <returns></returns>
    [PrivateApi("experimental in 12.05")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    (IValue ValueField, object Result) GetTypedValue(string[] languageKeys, bool fallbackToAny);

    [PrivateApi("internal only")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IAttribute With(IImmutableList<IValue> values);
}