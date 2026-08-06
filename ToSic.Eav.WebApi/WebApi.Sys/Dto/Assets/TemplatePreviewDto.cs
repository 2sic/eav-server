namespace ToSic.Eav.WebApi.Sys.Dto;

public record TemplatePreviewDto
{
    public bool IsValid => string.IsNullOrEmpty(Error);

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    public string? Preview { get; init; }
}