namespace ToSic.Eav.WebApi.Dto;

public class ContextLanguageDto
{
    public string Primary { get; set; }
    public string Current { get; set; }
    public List<SiteLanguageDto> List { get; set; }
}