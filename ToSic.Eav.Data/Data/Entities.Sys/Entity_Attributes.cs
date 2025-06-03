using System.Collections.Immutable;

namespace ToSic.Eav.Data;

partial record Entity
{
    /// <inheritdoc />
    public required IImmutableDictionary<string, IAttribute> Attributes { get; init; }

    /// <inheritdoc />
    public IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

}