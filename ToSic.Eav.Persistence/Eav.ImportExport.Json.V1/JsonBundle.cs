namespace ToSic.Eav.ImportExport.Json.V1;

/// <summary>
/// WIP new in v15
///
/// Should contain a set of things to preserve together
/// </summary>
public record JsonBundle
{
    public string Name { get; init; } = "default";

    [JsonIgnore(Condition = WhenWritingDefault)]
    public ICollection<JsonContentTypeSet>? ContentTypes { get; init; }

    [JsonIgnore(Condition = WhenWritingDefault)]
    public ICollection<JsonEntity>? Entities { get; init; }
}