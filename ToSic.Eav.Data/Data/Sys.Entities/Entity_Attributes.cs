namespace ToSic.Eav.Data.Sys.Entities;

partial record Entity
{
    /// <inheritdoc />
    public required IReadOnlyDictionary<string, IAttribute> Attributes { get; init; }

    /// <inheritdoc />
    public IAttribute? this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

}