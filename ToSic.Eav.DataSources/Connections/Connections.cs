using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi("Experimental")]
    public class Connections
    {
        public List<Connection> In = new List<Connection>();
        public List<Connection> Out = new List<Connection>();

        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public DataSourceBase Parent { get; }

        
        public Connections(DataSourceBase parent) => Parent = parent;

        public void AddIn(Connection connection)
        {
            // Check if a connection was already added?
            var existing = In.FirstOrDefault(item => item.SourceStream == connection.SourceStream);
            if (existing != null) In.Remove(existing);
                
            In.Add(connection);
            (connection.DataSource as DataSourceBase)?.Connections.Out.Add(connection);
        }

        public void ClearIn()
        {
            // Remove from Out of other side
            foreach (var connection in In)
                (connection?.DataSource as DataSourceBase)?.Connections?.Out.Remove(connection);
            
            In.Clear();
            
            Parent.In.Clear();
        }
        
    }
}
