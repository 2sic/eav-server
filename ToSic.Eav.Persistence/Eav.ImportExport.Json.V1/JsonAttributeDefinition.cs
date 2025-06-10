

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonAttributeDefinition
{
    [JsonPropertyOrder(1)]
    public string Name;

    /// <summary>
    /// WIP in 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(3)]
    public Guid? Guid { get; set; }

    [JsonPropertyOrder(7)]
    public string Type;

    [JsonPropertyOrder(8)]
    public string InputType;

    [JsonPropertyOrder(9)]
    public bool IsTitle;

    /// <summary>
    /// WIP in 16.08+
    /// #SharedFieldDefinition
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(10)]
    public JsonAttributeSysSettings SysSettings { get; set; }

    /// <summary>
    /// Metadata Entities for this Attribute such as description, dropdown values etc.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(100)]
    public ICollection<JsonEntity> Metadata;
}