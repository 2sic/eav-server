namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonAttributes
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string?>>? String { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)] public Dictionary<string, Dictionary<string, string?>>? Hyperlink { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string?>>? Custom { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string?>>? Json { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, ICollection<Guid?>>>? Entity { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, decimal?>>? Number { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, DateTime?>>? DateTime { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public Dictionary<string, Dictionary<string, bool?>>? Boolean { get; init; }
}