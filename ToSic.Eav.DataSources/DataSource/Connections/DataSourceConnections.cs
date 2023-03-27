using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    [PrivateApi]
    public class DataSourceConnections
    {
        public List<DataSourceConnection> In = new List<DataSourceConnection>();
        public List<DataSourceConnection> Out = new List<DataSourceConnection>();

        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public DataSources.DataSource Parent { get; }

        
        public DataSourceConnections(DataSources.DataSource parent) => Parent = parent;

        public void AddIn(DataSourceConnection connection)
        {
            // Check if a connection was already added?
            var existing = In.FirstOrDefault(item => item.SourceStream == connection.SourceStream);
            if (existing != null) In.Remove(existing);
                
            In.Add(connection);
            (connection.DataSource as DataSources.DataSource)?.Connections.Out.Add(connection);
        }

        public void ClearIn()
        {
            // Remove from Out of other side
            foreach (var connection in In)
                (connection?.DataSource as DataSources.DataSource)?.Connections?.Out.Remove(connection);
            
            In.Clear();
            
            Parent.In.Clear();
        }
        
    }
}
