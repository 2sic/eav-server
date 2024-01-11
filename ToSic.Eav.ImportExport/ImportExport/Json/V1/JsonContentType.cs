using static System.Text.Json.Serialization.JsonIgnoreCondition;

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

    // TODO: Don't just remove, it's possible that we're using it in the admin-UI...
    // Review w/2dm before removing this code
    // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(99)]
    public string Description;
}