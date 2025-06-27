namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonContentTypeShareable
{
    [JsonIgnore(Condition = WhenWritingDefault)] public bool AlwaysShare { get; init; }
    [JsonIgnore(Condition = WhenWritingDefault)] public int ParentZoneId { get; init; }
    [JsonIgnore(Condition = WhenWritingDefault)] public int ParentAppId { get; init; }
    [JsonIgnore(Condition = WhenWritingDefault)] public int? ParentId { get; init; }
}