using System.Collections.Immutable;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents an Attribute Definition.
/// Used in Content-Types and IEntities.
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// * We recommend you read about the [](xref:Basics.Data.Index)
/// * Changed to be a record in v19.01
/// </remarks>
[PrivateApi("Hidden in 12.04 2021-09 because people should only use the interface - previously InternalApi. This is just fyi, use Interface IAttributeBase")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record AttributeBase : IAttributeBase
{
    /// <inheritdoc />
    public required string Name { get; init; }

    /// <inheritdoc />
    public required ValueTypes Type { get; init; }

    /// <summary>
    /// Empty values are always the same, and immutable, so create once only for speed & memory use.
    /// </summary>
    internal static readonly IImmutableList<IValue> EmptyValues = new List<IValue>().ToImmutableList();
}