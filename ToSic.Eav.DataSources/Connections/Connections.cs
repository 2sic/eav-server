using System.Collections.Generic;
using Newtonsoft.Json;
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
            // todo: maybe check if a similar / identical connection was already added?
            
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
