namespace ToSic.Eav.WebApi.Sys.Dto;

public record ContextLanguageInfoDto
{
    public required string Primary { get; init; }
    public required string Current { get; init; }
    public required List<Context.ContextLanguageDto> List { get; init; }
}
