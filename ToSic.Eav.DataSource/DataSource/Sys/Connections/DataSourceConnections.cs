using System.Text.Json.Serialization;

namespace ToSic.Eav.DataSource.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataSourceConnections(DataSourceBase parent)
{
    public List<DataSourceConnection> In = [];
    public List<DataSourceConnection> Out = [];

    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public DataSourceBase Parent { get; } = parent;


    public void AddIn(DataSourceConnection connection)
    {
        // Check if a connection was already added?
        var existing = In.FirstOrDefault(item => item.SourceStream == connection.SourceStream);
        if (existing != null) In.Remove(existing);
                
        In.Add(connection);
        (connection.DataSource as DataSourceBase)?.Connections.Out.Add(connection);
    }
        
}