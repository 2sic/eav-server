namespace ToSic.Eav.DataSource.Internal.Query;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryDefinitionDto
{
    public Dictionary<string, object> Pipeline { get; set; }
    public List<Dictionary<string, object>> DataSources { get; set; } = [];
}