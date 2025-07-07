

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonAttributeDefinition
{
    [JsonPropertyOrder(1)]
    public required string Name { get; init; }

    /// <summary>
    /// WIP in 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(3)]
    public Guid? Guid { get; init; }

    [JsonPropertyOrder(7)]
    public required string Type { get; init; }

    /// <summary>
    /// Note: can be null for older exported json files, so not required, otherwise things fail.
    /// </summary>
    [JsonPropertyOrder(8)]
    public string? InputType { get; init; }

    [JsonPropertyOrder(9)]
    public bool IsTitle { get; init; }

    /// <summary>
    /// WIP in 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(10)]
    public JsonAttributeSysSettings? SysSettings { get; init; }

    /// <summary>
    /// Metadata Entities for this Attribute such as description, dropdown values etc.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(100)]
    public ICollection<JsonEntity>? Metadata { get; init; }
}