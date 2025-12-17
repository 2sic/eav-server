

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

    /// <summary>
    /// WIP to store inbound parent relationships and the position they use to reference this child
    /// only used for JSON in history.
    /// </summary>
    /// <remarks>
    /// WIP v21
    /// </remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public List<JsonRelationship>? Parents { get; init; }
}

/// <summary>
/// WIP
/// </summary>
public record JsonRelationship
{
    /// <summary>
    /// Guid of the parent
    /// </summary>
    public required Guid Parent { get; init; }

    /// <summary>
    /// Name of the field for this relationship, like "Author" or "Photographer" - since a *Person* entity could be used in both.
    /// </summary>
    public required string Field { get; init; }

    /// <summary>
    /// Position in the list of that field, so the Author #2, etc.
    /// </summary>
    public required int SortOrder { get; init; }
}