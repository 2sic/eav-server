using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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

    //public void ClearIn()
    //{
    //    // Remove from Out of other side
    //    foreach (var connection in In)
    //        (connection?.DataSource as DataSourceBase)?.Connections?.Out.Remove(connection);
            
    //    In.Clear();
            
    //    Parent.In.Clear();
    //}
        
}