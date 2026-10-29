namespace ToSic.Eav.ImportExport.Json.V1;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record JsonFormat: JsonContentTypeSet
{
    /// <summary>
    /// V1 - header information
    /// </summary>
    [JsonPropertyOrder(-1000)] // make sure it's always on top for clarity
    public JsonHeader _ = new();
        
    /// <summary>
    /// Bundles in this package.
    /// Added ca. v15
    /// </summary>
    [JsonIgnore(Condition = WhenWritingDefault)]
    [JsonPropertyOrder(10)]
    public List<JsonBundle>? Bundles { get; init; }

    /// <summary>
    /// V1 - a single Entity
    /// </summary>
    [JsonPropertyOrder(20)]
    [JsonIgnore(Condition = WhenWritingNull)] 
    public JsonEntity? Entity { get; init; }
}