namespace ToSic.Eav.WebApi.Sys.Dto;

public class TemplatePreviewDto
{
    public bool IsValid => string.IsNullOrEmpty(Error);

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    public string? Preview { get; set; }
}