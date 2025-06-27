namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonContentType: IJsonWithAssets
{
    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-100)]   // use negative indexes to ensure that all without Prop-order come afterward
    public required string Id { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-90)]
    public required string Name { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-80)]
    public required string Scope { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-70)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public JsonContentTypeShareable? Sharing { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-60)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public ICollection<JsonEntity>? Metadata { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-50)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public ICollection<JsonAttributeDefinition>? Attributes { get; init; }

    public ICollection<JsonAttributeDefinition> AttributesSafe()
        => Attributes ?? [];

    /// <remarks>V 1.1</remarks>
    [JsonPropertyOrder(-40)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public ICollection<JsonAsset>? Assets { get; init; }
}

public record JsonContentTypeWithTitleWip: JsonContentType
{
    /// <summary>
    /// The title of the content type.
    /// Temporary, as it's used in the UI.
    /// This implementation is probably not final at all
    /// and should never become part of the official export/import format.
    /// </summary>
    [JsonPropertyOrder(-10)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Title { get; init; }
}