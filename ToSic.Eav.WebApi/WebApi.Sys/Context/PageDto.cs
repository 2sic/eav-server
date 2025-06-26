namespace ToSic.Eav.WebApi.Sys.Context;

public class PageDto
{
    public required int Id;
    public required string CultureCode;
    public required string Name;
    public required string Title;
    public required string Url;
    public required bool Visible;
    public required SiteDto Portal;
}