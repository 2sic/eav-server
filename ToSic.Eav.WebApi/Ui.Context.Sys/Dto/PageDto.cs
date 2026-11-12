namespace ToSic.Eav.Ui.Context.Sys.Dto;

public class PageDto
{
    public required int Id { get; init; }
    public required string CultureCode { get; init; }
    public required string Name { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required bool Visible { get; init; }
    public required SiteDto Portal { get; init; }
}