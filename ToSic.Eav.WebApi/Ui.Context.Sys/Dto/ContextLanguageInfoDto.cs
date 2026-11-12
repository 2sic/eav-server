namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ContextLanguageInfoDto
{
    public required string Primary { get; init; }
    public required string Current { get; init; }
    public required List<ContextLanguageDto> List { get; init; }
}
