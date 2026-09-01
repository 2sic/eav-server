namespace ToSic.Eav.WebApi.Sys.Cms;

public record ReplacementListDto
{
    [JsonPropertyName("selectedId")]
    public int? SelectedId { get; init; }

    [JsonPropertyName("items")]
    public required IEnumerable<ReplacementListItemDto> Items { get; init; }
}

public record ReplacementListItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    [JsonPropertyName("contentType")]
    public required string ContentType { get; init; }
}