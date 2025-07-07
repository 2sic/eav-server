namespace ToSic.Eav.WebApi.Sys.Admin.Query;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record QueryDefinitionDto
{
    public Dictionary<string, object> Pipeline { get; init; } = [];
    public IList<Dictionary<string, object>> DataSources { get; init; } = [];
}