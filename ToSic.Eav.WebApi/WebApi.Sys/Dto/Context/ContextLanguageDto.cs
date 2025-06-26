namespace ToSic.Eav.WebApi.Sys.Dto;

public class ContextLanguageDto
{
    public required string Primary { get; init; }
    public required string Current { get; init; }
    public required List<SiteLanguageDto> List { get; init; }
}