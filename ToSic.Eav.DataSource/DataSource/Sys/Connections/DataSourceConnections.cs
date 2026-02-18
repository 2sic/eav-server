namespace ToSic.Eav.DataSource.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class DataSourceConnections
{
    public List<DataSourceConnection> In = [];
    public List<DataSourceConnection> Out = [];

    /// <summary>
    /// Keep track of connections for inspection in Visual Query.
    /// </summary>
    /// <param name="connection"></param>
    internal void RegisterForInspection(DataSourceConnection connection)
    {
        // Check if a connection was already added?
        var existing = In.FirstOrDefault(item => item.SourceStream == connection.SourceStream);
        if (existing != null)
            In.Remove(existing);
                
        In.Add(connection);

        // Also add out to counterparty connections (on the original source)
        (connection.Source as DataSourceBase)?.Connections.Out.Add(connection);
    }
}