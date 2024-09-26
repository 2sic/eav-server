namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonContentType: IJsonWithAssets
{
    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-100)]   // use negative indexes to ensure that all without Prop-order come afterwards
    public string Id;

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-90)]
    public string Name;

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-80)]
    public string Scope;

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-70)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public JsonContentTypeShareable Sharing;

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-60)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public List<JsonEntity> Metadata;

    /// <remarks>V 1.0</remarks>
    [JsonPropertyOrder(-50)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public List<JsonAttributeDefinition> Attributes;

    /// <remarks>V 1.1</remarks>
    [JsonPropertyOrder(-40)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public List<JsonAsset> Assets { get; set; }
}

public class JsonContentTypeWithTitleWip: JsonContentType
{
    /// <summary>
    /// The title of the content type.
    /// Temporary, as it's used in the UI.
    /// This implementation is probably not final at all
    /// and should never become part of the official export/import format.
    /// </summary>
    [JsonPropertyOrder(-10)]
    [JsonIgnore(Condition = WhenWritingNull)]
    public string Title;
}