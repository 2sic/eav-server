namespace ToSic.Eav.DataSource.Sys.Inspect;

public class DataSourceConnectionsDto
{
    public List<DataSourceConnectionDto> In { get; init; }= [];
    public List<DataSourceConnectionDto> Out { get; init; }= [];

}

public class DataSourceConnectionDto
{
    internal DataSourceConnectionDto(DataSourceConnection connection)
    {
        Source = new(connection.Source, connection.SourceStream);
        Target = new(connection.Target, connection.TargetStream);
    }

    public DataSourceInfoDto Source { get; }
    public DataSourceInfoDto Target { get; }

}

public class DataSourceInfoDto(IDataSource dataSource, string streamName)
{
    public string? Label { get; } = dataSource?.Label;
    public Guid? Guid { get; } = dataSource?.Guid;
    public string? Name { get; } = dataSource?.Name;
    public string? Stream { get; } = streamName;
}