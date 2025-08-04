namespace ToSic.Eav.ImportExport.Json.Schema;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Todo: returned mime type should be "application/schema+json"
/// </remarks>
public class JsonSchemaRoot: JsonSchema
{
    /// <summary>
    /// Header / Version information.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string Schema { get; init; } = "https://json-schema.org/draft/2020-12/schema";

    /// <summary>
    /// The json-schema ID; TODO: Content-Type ID
    /// </summary>
    [JsonPropertyName("$id")]
    public required string Id { get; init; }


}