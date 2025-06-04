

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonEntity: IJsonWithAssets
{
    /// <remarks>V 1.0</remarks>
    public int Id;

    /// <remarks>V 1.0</remarks>
    public int Version;

    /// <remarks>V 1.0</remarks>
    public Guid Guid;

    /// <remarks>V 1.0</remarks>
    public JsonType Type;

    /// <remarks>V 1.0</remarks>
    public JsonAttributes Attributes;

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)] public string Owner;

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)] public JsonMetadataFor For;

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)] public List<JsonEntity> Metadata;

    /// <remarks>V 1.1</remarks>
    [JsonIgnore(Condition = WhenWritingNull)] public List<JsonAsset> Assets { get; set; }
}