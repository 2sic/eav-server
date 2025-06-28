

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonEntity: IJsonWithAssets
{
    /// <remarks>V 1.0</remarks>
    public int Id { get; init; }

    /// <remarks>V 1.0</remarks>
    public int Version { get; init; }

    /// <remarks>V 1.0</remarks>
    public Guid Guid { get; init; }

    /// <remarks>V 1.0</remarks>
    public required JsonType Type { get; init; }

    /// <remarks>V 1.0</remarks>
    public required JsonAttributes Attributes { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Owner { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public JsonMetadataFor? For { get; init; }

    /// <remarks>V 1.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public ICollection<JsonEntity>? Metadata { get; init; }

    /// <remarks>V 1.1</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public ICollection<JsonAsset>? Assets { get; init; }
}