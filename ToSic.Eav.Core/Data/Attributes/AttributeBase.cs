using System.Collections.Immutable;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents an Attribute Definition.
/// Used in Content-Types and IEntities.
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// * We recommend you read about the [](xref:Basics.Data.Index)
/// </remarks>
[PrivateApi("Hidden in 12.04 2021-09 because people should only use the interface - previously InternalApi. This is just fyi, use Interface IAttributeBase")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AttributeBase : IAttributeBase
{
    /// <summary>
    /// Extended constructor when also storing the persistence ID-Info
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected AttributeBase(string name, ValueTypes type)
    {
        Name = name;
        Type = type;
    }

    /// <inheritdoc />
    public string Name { get; }

    public ValueTypes Type { get; }


    /// <summary>
    /// Empty values are always the same, and immutable, so create once only for speed & memory use.
    /// </summary>
    protected static readonly IImmutableList<IValue> EmptyValues = new List<IValue>().ToImmutableList();
}