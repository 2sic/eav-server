namespace ToSic.Eav.DataSource.Query.Sys.Inspect;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryConnectionsDto
{
    public List<QueryConnectionDto> In { get; init; }= [];
    public List<QueryConnectionDto> Out { get; init; }= [];

}

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryConnectionDto
{
    internal QueryConnectionDto(DataSourceConnection connection)
    {
        Source = new(connection.Source, connection.SourceStream);
        Target = new(connection.Target, connection.TargetStream);
    }

    public QueryConnectionSourceDto Source { get; }
    public QueryConnectionSourceDto Target { get; }

}

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryConnectionSourceDto(IDataSource dataSource, string streamName)
{
    public string? Label { get; } = dataSource?.Label;
    public Guid? Guid { get; } = dataSource?.Guid;
    public string? Name { get; } = dataSource?.Name;
    public string? Stream { get; } = streamName;
}