using System.Collections.Immutable;

namespace ToSic.Eav.Data;

partial class Entity
{
    /// <inheritdoc />
    public IImmutableDictionary<string, IAttribute> Attributes => _attributes;
    private readonly IImmutableDictionary<string, IAttribute> _attributes;

    /// <inheritdoc />
    public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

}