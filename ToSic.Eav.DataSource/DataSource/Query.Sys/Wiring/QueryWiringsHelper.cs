namespace ToSic.Eav.DataSource.Query.Sys;

internal class QueryWiringsHelper(ILog? parentLog) : HelperBase(parentLog, "DS.QWireH")
{

    /// <summary>
    /// Init Stream Wirings between Query-Parts (Bottom-Up)
    /// </summary>
    internal void InitWirings(QueryDefinition queryDef, IDictionary<string, IDataSource> dataSources)
    {
        var l = Log.Fn($"count⋮{queryDef.Connections?.Count}");
        // Init
        var wirings = queryDef.Connections ?? [];
        //var initializedWirings = new List<Connection>();

        // 1. wire Out-Streams of DataSources with no In-Streams
        var dataSourcesWithNoInStreams = dataSources
            .Where(d => wirings.All(w => w.To != d.Key));

        var (connectionsWereAdded, initializedWirings) = ConnectOutStreams(dataSourcesWithNoInStreams, dataSources, wirings, []);

        // 2. init DataSources with In-Streams of DataSources which are already wired
        // note: there is a bug here, because when a DS has "In" from multiple sources, then it won't always be ready to provide out...
        // repeat until all are connected
        //var connectionsWereAdded = true;
        while (connectionsWereAdded)
        {
            var dataSourcesWithInitializedInStreams = dataSources
                .Where(d => initializedWirings.Any(w => w.To == d.Key))
                .ToList();

            (connectionsWereAdded, initializedWirings) = ConnectOutStreams(dataSourcesWithInitializedInStreams, dataSources, wirings, initializedWirings);
        }

        // 3. Test all Wirings were created
        if (wirings.Count != initializedWirings.Count)
        {
            var notInitialized = wirings
                .Where(w => !initializedWirings.Any(i => i.Equals(w)));
            var error = string.Join(", ", notInitialized);
            var exception = new Exception("Some Stream-Wirings were not created: " + error);
            l.Ex(exception);
            throw exception;
        }

        l.Done();
    }

    /// <summary>
    /// Wire all Out-Wirings on specified DataSources
    /// </summary>
    internal static (bool ConnectionsWereAdded, List<QueryWire> InitializedWirings) ConnectOutStreams(
        IEnumerable<KeyValuePair<string, IDataSource>> dataSourcesToInit,
        IDictionary<string, IDataSource> allDataSources,
        IList<QueryWire> allWirings,
        List<QueryWire> initializedWirings)
    {
        var connectionsWereAdded = false;

        foreach (var dataSource in dataSourcesToInit)
        {
            var unassignedConnectionsForThisSource = allWirings
                .Where(w => w.From == dataSource.Key && !initializedWirings.Any(i => i.Equals(w)))
                .ToArray();

            // loop all wirings from this DataSource (except already initialized)
            connectionsWereAdded = unassignedConnectionsForThisSource
                .Aggregate(connectionsWereAdded, (current, wire) => current | AddConnectionFromWire(wire));

            initializedWirings = initializedWirings
                .Concat(unassignedConnectionsForThisSource)
                .ToList();
        }

        return (connectionsWereAdded, initializedWirings);

        bool AddConnectionFromWire(QueryWire wire)
        {
            var errMsg = $"Trouble with connecting query from {wire.From}:{wire.Out} to {wire.To}:{wire.In}. ";
            if (!allDataSources.TryGetValue(wire.From, out var conSource))
                throw new(errMsg + $"The source '{wire.From}' can't be found");
            if (!allDataSources.TryGetValue(wire.To, out var conTarget))
                throw new(errMsg + $"The target '{wire.To}' can't be found");

            try
            {
                // Temporary solution until immutable works perfectly
                if (wire.In != "*")
                    conTarget.DoWhileOverrideImmutable(() => conTarget.Attach(wire.In, conSource, wire.Out));
                else
                    conTarget.DoWhileOverrideImmutable(() => conTarget.Attach(conSource));
                
                //initializedWirings.Add(wire);
                // In the end, inform caller that we did add some connections
                return true;
            }
            catch (Exception ex)
            {
                throw new(errMsg, ex);
            }
        }

    }
}
